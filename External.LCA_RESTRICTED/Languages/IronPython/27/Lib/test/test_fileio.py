# Adapted from test_file.py by Daniel Stutzbach

from __future__ import unicode_literals

import sys
import os
import errno
import unittest
from array import array
from weakref import proxy
from functools import wraps

from test.test_support import (TESTFN, check_warnings, run_unittest, make_bad_fd,
                               due_to_ironpython_bug, gc_collect)
from test.test_support import py3k_bytes as bytes
if not due_to_ironpython_bug("http://ironpython.codeplex.com/workitem/15512"):
    from test.script_helper import run_python

from _io import FileIO as _FileIO

class AutoFileTests(unittest.TestCase):
    # file tests for which a test file is automatically set up

    def setUp(self):
        self.f = _FileIO(TESTFN, 'w')

    def tearDown(self):
        if self.f:
            self.f.close()
        os.remove(TESTFN)

    def testWeakRefs(self):
        if due_to_ironpython_bug("http://tkbgitvstfat01:8080/WorkItemTracking/WorkItem.aspx?artifactMoniker=313703"):
            return
        # verify weak references
        p = proxy(self.f)
        p.write(bytes(range(10)))
        self.assertEquals(self.f.tell(), p.tell())
        self.f.close()
        self.f = None
        gc_collect()
        self.assertRaises(ReferenceError, getattr, p, 'tell')

    def testSeekTell(self):
        self.f.write(bytes(range(20)))
        self.assertEquals(self.f.tell(), 20)
        self.f.seek(0)
        self.assertEquals(self.f.tell(), 0)
        self.f.seek(10)
        self.assertEquals(self.f.tell(), 10)
        self.f.seek(5, 1)
        self.assertEquals(self.f.tell(), 15)
        self.f.seek(-5, 1)
        self.assertEquals(self.f.tell(), 10)
        self.f.seek(-5, 2)
        self.assertEquals(self.f.tell(), 15)

    def testAttributes(self):
        # verify expected attributes exist
        f = self.f

        self.assertEquals(f.mode, "wb")
        self.assertEquals(f.closed, False)

        # verify the attributes are readonly
        for attr in 'mode', 'closed':
            self.assertRaises((AttributeError, TypeError),
                              setattr, f, attr, 'oops')

    def testReadinto(self):
        # verify readinto
        self.f.write(b"\x01\x02")
        self.f.close()
        a = array(b'b', b'x'*10)
        self.f = _FileIO(TESTFN, 'r')
        n = self.f.readinto(a)
        self.assertEquals(array(b'b', [1, 2]), a[:n])

    def test_none_args(self):
        self.f.write(b"hi\nbye\nabc")
        self.f.close()
        self.f = _FileIO(TESTFN, 'r')
        self.assertEqual(self.f.read(None), b"hi\nbye\nabc")
        self.f.seek(0)
        self.assertEqual(self.f.readline(None), b"hi\n")
        self.assertEqual(self.f.readlines(None), [b"bye\n", b"abc"])

    def testRepr(self):
        self.assertEquals(repr(self.f), "<_io.FileIO name=%r mode='%s'>"
                                        % (self.f.name, self.f.mode))
        if due_to_ironpython_bug("http://ironpython.codeplex.com/workitem/22896"):
            return
        del self.f.name
        self.assertEquals(repr(self.f), "<_io.FileIO fd=%r mode='%s'>"
                                        % (self.f.fileno(), self.f.mode))
        self.f.close()
        self.assertEquals(repr(self.f), "<_io.FileIO [closed]>")

    def testErrors(self):
        f = self.f
        self.assertTrue(not f.isatty())
        self.assertTrue(not f.closed)
        #self.assertEquals(f.name, TESTFN)
        self.assertRaises(ValueError, f.read, 10) # Open for reading
        f.close()
        self.assertTrue(f.closed)
        f = _FileIO(TESTFN, 'r')
        self.assertRaises(TypeError, f.readinto, "")
        self.assertTrue(not f.closed)
        f.close()
        self.assertTrue(f.closed)

    def testMethods(self):
        methods = {
            'fileno' : (),
            'isatty' : (),
            'read' : (),
            'readinto' : (array('b', ''),),
            'seek' : (0,),
            'tell' : (),
            'truncate' : (),
            'write' : (b'',),
            'seekable' : (),
            'readable' : (),
            'writable' : (),
            }
        
        if sys.platform.startswith('atheos'):
            methods.remove('truncate')

        self.f.close()
        self.assertTrue(self.f.closed)

        for methodname in methods.keys():
            method = getattr(self.f, methodname)
            # should raise on closed file
            self.assertRaises(ValueError, method, *methods[methodname])

    def testOpendir(self):
        # Issue 3703: opening a directory should fill the errno
        # Windows always returns "[Errno 13]: Permission denied
        # Unix calls dircheck() and returns "[Errno 21]: Is a directory"
        try:
            _FileIO('.', 'r')
        except IOError as e:
            self.assertNotEqual(e.errno, 0)
            self.assertEqual(e.filename, ".")
        else:
            self.fail("Should have raised IOError")

    #A set of functions testing that we get expected behaviour if someone has
    #manually closed the internal file descriptor.  First, a decorator:
    def ClosedFD(func):
        @wraps(func)
        def wrapper(self):
            #forcibly close the fd before invoking the problem function
            f = self.f
            if due_to_ironpython_bug("http://ironpython.codeplex.com/workitem/22896"):
                f.close()
            else:
                os.close(f.fileno())
            try:
                func(self, f)
            except ValueError:
                if not due_to_ironpython_bug("http://ironpython.codeplex.com/workitem/22896"):
                    raise
            finally:
                try:
                    self.f.close()
                except IOError:
                    pass
        return wrapper

    def ClosedFDRaises(func):
        @wraps(func)
        def wrapper(self):
            #forcibly close the fd before invoking the problem function
            f = self.f
            if due_to_ironpython_bug("http://ironpython.codeplex.com/workitem/22896"):
                f.close()
            else:
                os.close(f.fileno())
            try:
                func(self, f)
            except IOError as e:
                self.assertEqual(e.errno, errno.EBADF)
            except (OSError, ValueError):
                if not due_to_ironpython_bug("http://ironpython.codeplex.com/workitem/22896"):
                    raise
            else:
                self.fail("Should have raised IOError")
            finally:
                try:
                    self.f.close()
                except IOError:
                    pass
        return wrapper

    if not due_to_ironpython_bug("http://ironpython.codeplex.com/workitem/22896"):
        @ClosedFDRaises
        def testErrnoOnClose(self, f):
            f.close()

    @ClosedFDRaises
    def testErrnoOnClosedWrite(self, f):
        f.write('a')

    @ClosedFDRaises
    def testErrnoOnClosedSeek(self, f):
        f.seek(0)

    @ClosedFDRaises
    def testErrnoOnClosedTell(self, f):
        f.tell()

    @ClosedFDRaises
    def testErrnoOnClosedTruncate(self, f):
        f.truncate(0)

    @ClosedFD
    def testErrnoOnClosedSeekable(self, f):
        f.seekable()

    @ClosedFD
    def testErrnoOnClosedReadable(self, f):
        f.readable()

    @ClosedFD
    def testErrnoOnClosedWritable(self, f):
        f.writable()

    @ClosedFD
    def testErrnoOnClosedFileno(self, f):
        f.fileno()

    @ClosedFD
    def testErrnoOnClosedIsatty(self, f):
        self.assertEqual(f.isatty(), False)

    def ReopenForRead(self):
        try:
            self.f.close()
        except IOError:
            pass
        self.f = _FileIO(TESTFN, 'r')
        os.close(self.f.fileno())
        return self.f

    @ClosedFDRaises
    def testErrnoOnClosedRead(self, f):
        f = self.ReopenForRead()
        f.read(1)

    @ClosedFDRaises
    def testErrnoOnClosedReadall(self, f):
        f = self.ReopenForRead()
        f.readall()

    @ClosedFDRaises
    def testErrnoOnClosedReadinto(self, f):
        f = self.ReopenForRead()
        a = array(b'b', b'x'*10)
        f.readinto(a)

class OtherFileTests(unittest.TestCase):

    def tearDown(self):
        gc_collect()

    def testAbles(self):
        try:
            f = _FileIO(TESTFN, "w")
            self.assertEquals(f.readable(), False)
            self.assertEquals(f.writable(), True)
            self.assertEquals(f.seekable(), True)
            f.close()

            f = _FileIO(TESTFN, "r")
            self.assertEquals(f.readable(), True)
            self.assertEquals(f.writable(), False)
            self.assertEquals(f.seekable(), True)
            f.close()

            f = _FileIO(TESTFN, "a+")
            self.assertEquals(f.readable(), True)
            self.assertEquals(f.writable(), True)
            self.assertEquals(f.seekable(), True)
            self.assertEquals(f.isatty(), False)
            f.close()

            if sys.platform != "win32":
                try:
                    f = _FileIO("/dev/tty", "a")
                except EnvironmentError:
                    # When run in a cron job there just aren't any
                    # ttys, so skip the test.  This also handles other
                    # OS'es that don't support /dev/tty.
                    pass
                else:
                    f = _FileIO("/dev/tty", "a")
                    self.assertEquals(f.readable(), False)
                    self.assertEquals(f.writable(), True)
                    if sys.platform != "darwin" and \
                       'bsd' not in sys.platform and \
                       not sys.platform.startswith('sunos'):
                        # Somehow /dev/tty appears seekable on some BSDs
                        self.assertEquals(f.seekable(), False)
                    self.assertEquals(f.isatty(), True)
                    f.close()
        finally:
            os.unlink(TESTFN)

    def testModeStrings(self):
        # check invalid mode strings
        for mode in ("", "aU", "wU+", "rw", "rt"):
            try:
                f = _FileIO(TESTFN, mode)
            except ValueError:
                pass
            else:
                f.close()
                self.fail('%r is an invalid file mode' % mode)

    def testUnicodeOpen(self):
        # verify repr works for unicode too
        f = _FileIO(str(TESTFN), "w")
        f.close()
        os.unlink(TESTFN)

    def testBytesOpen(self):
        # Opening a bytes filename
        try:
            fn = TESTFN.encode("ascii")
        except UnicodeEncodeError:
            # Skip test
            return
        f = _FileIO(fn, "w")
        try:
            f.write(b"abc")
            f.close()
            with open(TESTFN, "rb") as f:
                self.assertEquals(f.read(), b"abc")
        finally:
            os.unlink(TESTFN)

    def testInvalidFd(self):
        self.assertRaises(ValueError, _FileIO, -10)
        self.assertRaises(OSError, _FileIO, make_bad_fd())

    def testBadModeArgument(self):
        # verify that we get a sensible error message for bad mode argument
        bad_mode = "qwerty"
        try:
            f = _FileIO(TESTFN, bad_mode)
        except ValueError as msg:
            if msg.args[0] != 0:
                s = str(msg)
                if TESTFN in s or bad_mode not in s:
                    self.fail("bad error message for invalid mode: %s" % s)
            # if msg.args[0] == 0, we're probably on Windows where there may be
            # no obvious way to discover why open() failed.
        else:
            f.close()
            self.fail("no error for invalid mode: %s" % bad_mode)

    def testTruncate(self):
        f = _FileIO(TESTFN, 'w')
        f.write(bytes(bytearray(range(10))))
        self.assertEqual(f.tell(), 10)
        f.truncate(5)
        self.assertEqual(f.tell(), 10)
        self.assertEqual(f.seek(0, os.SEEK_END), 5)
        f.truncate(15)
        self.assertEqual(f.tell(), 5)
        self.assertEqual(f.seek(0, os.SEEK_END), 15)

    def testTruncateOnWindows(self):
        def bug801631():
            # SF bug <http://www.python.org/sf/801631>
            # "file.truncate fault on windows"
            f = _FileIO(TESTFN, 'w')
            f.write(bytes(range(11)))
            f.close()

            f = _FileIO(TESTFN,'r+')
            data = f.read(5)
            if data != bytes(range(5)):
                self.fail("Read on file opened for update failed %r" % data)
            if f.tell() != 5:
                self.fail("File pos after read wrong %d" % f.tell())

            f.truncate()
            if f.tell() != 5:
                self.fail("File pos after ftruncate wrong %d" % f.tell())

            f.close()
            size = os.path.getsize(TESTFN)
            if size != 5:
                self.fail("File size after ftruncate wrong %d" % size)

        try:
            bug801631()
        finally:
            gc_collect()
            os.unlink(TESTFN)

    def testAppend(self):
        try:
            f = open(TESTFN, 'wb')
            f.write(b'spam')
            f.close()
            f = open(TESTFN, 'ab')
            f.write(b'eggs')
            f.close()
            f = open(TESTFN, 'rb')
            d = f.read()
            f.close()
            self.assertEqual(d, b'spameggs')
        finally:
            try:
                os.unlink(TESTFN)
            except:
                pass

    def testInvalidInit(self):
        self.assertRaises(TypeError, _FileIO, "1", 0, 0)

    def testWarnings(self):
        with check_warnings(quiet=True) as w:
            self.assertEqual(w.warnings, [])
            self.assertRaises(TypeError, _FileIO, [])
            self.assertEqual(w.warnings, [])
            self.assertRaises(ValueError, _FileIO, "/some/invalid/name", "rt")
            self.assertEqual(w.warnings, [])

    def test_surrogates(self):
        if due_to_ironpython_bug("http://ironpython.codeplex.com/workitem/15512"):
            return
        # Issue #8438: try to open a filename containing surrogates.
        # It should either fail because the file doesn't exist or the filename
        # can't be represented using the filesystem encoding, but not because
        # of a LookupError for the error handler "surrogateescape".
        filename = u'\udc80.txt'
        try:
            with _FileIO(filename):
                pass
        except (UnicodeEncodeError, IOError):
            pass
        # Spawn a separate Python process with a different "file system
        # default encoding", to exercise this further.
        env = dict(os.environ)
        env[b'LC_CTYPE'] = b'C'
        _, out = run_python('-c', 'import _io; _io.FileIO(%r)' % filename, env=env)
        if ('UnicodeEncodeError' not in out and
            'IOError: [Errno 2] No such file or directory' not in out):
            self.fail('Bad output: %r' % out)

def test_main():
    # Historically, these tests have been sloppy about removing TESTFN.
    # So get rid of it no matter what.
    try:
        run_unittest(AutoFileTests, OtherFileTests)
    finally:
        if os.path.exists(TESTFN):
            os.unlink(TESTFN)

if __name__ == '__main__':
    test_main()
