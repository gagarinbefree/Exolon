import io
import pathlib
import tempfile
import unittest
import zipfile
from unittest.mock import patch

import get_android_template as template


class Response(io.BytesIO):
    status = 206

    def __init__(self, data):
        super().__init__(data)
        self.headers = {"Content-Range": f"bytes 0-{len(data) - 1}/{len(data)}"}


class TemplateDownloadTests(unittest.TestCase):
    def setUp(self):
        test_root = pathlib.Path(__file__).resolve().parent.parent / "artifacts"
        test_root.mkdir(exist_ok=True)
        self.directory = tempfile.TemporaryDirectory(dir=test_root)
        self.addCleanup(self.directory.cleanup)
        self.destination = pathlib.Path(self.directory.name) / "android_release.apk"
        self.payload = b"android template" * 200
        archive = io.BytesIO()
        with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as zf:
            zf.writestr("templates/android_release.apk", self.payload)
            entry = zf.getinfo("templates/android_release.apk")
        self.archive = archive.getvalue()
        self.constants = patch.multiple(template, START=0, END=len(self.archive) - 1,
                                       COMPRESSED_SIZE=entry.compress_size, SIZE=entry.file_size, CRC32=entry.CRC)
        self.constants.start()
        self.addCleanup(self.constants.stop)

    def test_timeout_then_success_and_verified_cache(self):
        with patch.object(template.urllib.request, "urlopen", side_effect=[TimeoutError("slow"), Response(self.archive)]) as request, \
                patch.object(template.time, "sleep"):
            template.download(self.destination)
            self.assertEqual(self.destination.read_bytes(), self.payload)
            self.assertEqual(request.call_count, 2)
            template.download(self.destination)
            self.assertEqual(request.call_count, 2)

    def test_failed_download_preserves_existing_file(self):
        self.destination.write_bytes(b"previous file")
        with patch.object(template.urllib.request, "urlopen", side_effect=TimeoutError("slow")) as request, \
                patch.object(template.time, "sleep"):
            with self.assertRaises(RuntimeError):
                template.download(self.destination)
        self.assertEqual(request.call_count, template.ATTEMPTS)
        self.assertEqual(self.destination.read_bytes(), b"previous file")

    def test_corrupt_download_is_not_installed(self):
        broken = bytearray(self.archive)
        broken[50] ^= 255
        with patch.object(template.urllib.request, "urlopen", side_effect=lambda *args, **kwargs: Response(broken)), \
                patch.object(template.time, "sleep"), self.assertRaises(RuntimeError):
            template.download(self.destination)
        self.assertFalse(self.destination.exists())


if __name__ == "__main__":
    unittest.main()
