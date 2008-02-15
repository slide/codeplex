/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime;

[assembly: PythonModule("errno", typeof(IronPython.Modules.PythonErrorNumber))]
namespace IronPython.Modules {
    public static class PythonErrorNumber {
        static PythonErrorNumber() {
            errorcode = new PythonDictionary();

            errorcode["E2BIG"] = E2BIG;
            errorcode["EACCES"] = EACCES;
            errorcode["EADDRINUSE"] = EADDRINUSE;
            errorcode["EADDRNOTAVAIL"] = EADDRNOTAVAIL;
            errorcode["EAFNOSUPPORT"] = EAFNOSUPPORT;
            errorcode["EAGAIN"] = EAGAIN;
            errorcode["EALREADY"] = EALREADY;
            errorcode["EBADF"] = EBADF;
            errorcode["EBUSY"] = EBUSY;
            errorcode["ECHILD"] = ECHILD;
            errorcode["ECONNABORTED"] = ECONNABORTED;
            errorcode["ECONNREFUSED"] = ECONNREFUSED;
            errorcode["ECONNRESET"] = ECONNRESET;
            errorcode["EDEADLK"] = EDEADLK;
            errorcode["EDEADLOCK"] = EDEADLOCK;
            errorcode["EDESTADDRREQ"] = EDESTADDRREQ;
            errorcode["EDOM"] = EDOM;
            errorcode["EDQUOT"] = EDQUOT;
            errorcode["EEXIST"] = EEXIST;
            errorcode["EFAULT"] = EFAULT;
            errorcode["EFBIG"] = EFBIG;
            errorcode["EHOSTDOWN"] = EHOSTDOWN;
            errorcode["EHOSTUNREACH"] = EHOSTUNREACH;
            errorcode["EILSEQ"] = EILSEQ;
            errorcode["EINPROGRESS"] = EINPROGRESS;
            errorcode["EINTR"] = EINTR;
            errorcode["EINVAL"] = EINVAL;
            errorcode["EIO"] = EIO;
            errorcode["EISCONN"] = EISCONN;
            errorcode["EISDIR"] = EISDIR;
            errorcode["ELOOP"] = ELOOP;
            errorcode["EMFILE"] = EMFILE;
            errorcode["EMLINK"] = EMLINK;
            errorcode["EMSGSIZE"] = EMSGSIZE;
            errorcode["ENAMETOOLONG"] = ENAMETOOLONG;
            errorcode["ENETDOWN"] = ENETDOWN;
            errorcode["ENETRESET"] = ENETRESET;
            errorcode["ENETUNREACH"] = ENETUNREACH;
            errorcode["ENFILE"] = ENFILE;
            errorcode["ENOBUFS"] = ENOBUFS;
            errorcode["ENODEV"] = ENODEV;
            errorcode["ENOENT"] = ENOENT;
            errorcode["ENOEXEC"] = ENOEXEC;
            errorcode["ENOLCK"] = ENOLCK;
            errorcode["ENOMEM"] = ENOMEM;
            errorcode["ENOPROTOOPT"] = ENOPROTOOPT;
            errorcode["ENOSPC"] = ENOSPC;
            errorcode["ENOSYS"] = ENOSYS;
            errorcode["ENOTCONN"] = ENOTCONN;
            errorcode["ENOTDIR"] = ENOTDIR;
            errorcode["ENOTEMPTY"] = ENOTEMPTY;
            errorcode["ENOTSOCK"] = ENOTSOCK;
            errorcode["ENOTTY"] = ENOTTY;
            errorcode["ENXIO"] = ENXIO;
            errorcode["EOPNOTSUPP"] = EOPNOTSUPP;
            errorcode["EPERM"] = EPERM;
            errorcode["EPFNOSUPPORT"] = EPFNOSUPPORT;
            errorcode["EPIPE"] = EPIPE;
            errorcode["EPROTONOSUPPORT"] = EPROTONOSUPPORT;
            errorcode["EPROTOTYPE"] = EPROTOTYPE;
            errorcode["ERANGE"] = ERANGE;
            errorcode["EREMOTE"] = EREMOTE;
            errorcode["EROFS"] = EROFS;
            errorcode["ESHUTDOWN"] = ESHUTDOWN;
            errorcode["ESOCKTNOSUPPORT"] = ESOCKTNOSUPPORT;
            errorcode["ESPIPE"] = ESPIPE;
            errorcode["ESRCH"] = ESRCH;
            errorcode["ESTALE"] = ESTALE;
            errorcode["ETIMEDOUT"] = ETIMEDOUT;
            errorcode["ETOOMANYREFS"] = ETOOMANYREFS;
            errorcode["EUSERS"] = EUSERS;
            errorcode["EWOULDBLOCK"] = EWOULDBLOCK;
            errorcode["EXDEV"] = EXDEV;
            errorcode["WSABASEERR"] = WSABASEERR;
            errorcode["WSAEACCES"] = WSAEACCES;
            errorcode["WSAEADDRINUSE"] = WSAEADDRINUSE;
            errorcode["WSAEADDRNOTAVAIL"] = WSAEADDRNOTAVAIL;
            errorcode["WSAEAFNOSUPPORT"] = WSAEAFNOSUPPORT;
            errorcode["WSAEALREADY"] = WSAEALREADY;
            errorcode["WSAEBADF"] = WSAEBADF;
            errorcode["WSAECONNABORTED"] = WSAECONNABORTED;
            errorcode["WSAECONNREFUSED"] = WSAECONNREFUSED;
            errorcode["WSAECONNRESET"] = WSAECONNRESET;
            errorcode["WSAEDESTADDRREQ"] = WSAEDESTADDRREQ;
            errorcode["WSAEDISCON"] = WSAEDISCON;
            errorcode["WSAEDQUOT"] = WSAEDQUOT;
            errorcode["WSAEFAULT"] = WSAEFAULT;
            errorcode["WSAEHOSTDOWN"] = WSAEHOSTDOWN;
            errorcode["WSAEHOSTUNREACH"] = WSAEHOSTUNREACH;
            errorcode["WSAEINPROGRESS"] = WSAEINPROGRESS;
            errorcode["WSAEINTR"] = WSAEINTR;
            errorcode["WSAEINVAL"] = WSAEINVAL;
            errorcode["WSAEISCONN"] = WSAEISCONN;
            errorcode["WSAELOOP"] = WSAELOOP;
            errorcode["WSAEMFILE"] = WSAEMFILE;
            errorcode["WSAEMSGSIZE"] = WSAEMSGSIZE;
            errorcode["WSAENAMETOOLONG"] = WSAENAMETOOLONG;
            errorcode["WSAENETDOWN"] = WSAENETDOWN;
            errorcode["WSAENETRESET"] = WSAENETRESET;
            errorcode["WSAENETUNREACH"] = WSAENETUNREACH;
            errorcode["WSAENOBUFS"] = WSAENOBUFS;
            errorcode["WSAENOPROTOOPT"] = WSAENOPROTOOPT;
            errorcode["WSAENOTCONN"] = WSAENOTCONN;
            errorcode["WSAENOTEMPTY"] = WSAENOTEMPTY;
            errorcode["WSAENOTSOCK"] = WSAENOTSOCK;
            errorcode["WSAEOPNOTSUPP"] = WSAEOPNOTSUPP;
            errorcode["WSAEPFNOSUPPORT"] = WSAEPFNOSUPPORT;
            errorcode["WSAEPROCLIM"] = WSAEPROCLIM;
            errorcode["WSAEPROTONOSUPPORT"] = WSAEPROTONOSUPPORT;
            errorcode["WSAEPROTOTYPE"] = WSAEPROTOTYPE;
            errorcode["WSAEREMOTE"] = WSAEREMOTE;
            errorcode["WSAESHUTDOWN"] = WSAESHUTDOWN;
            errorcode["WSAESOCKTNOSUPPORT"] = WSAESOCKTNOSUPPORT;
            errorcode["WSAESTALE"] = WSAESTALE;
            errorcode["WSAETIMEDOUT"] = WSAETIMEDOUT;
            errorcode["WSAETOOMANYREFS"] = WSAETOOMANYREFS;
            errorcode["WSAEUSERS"] = WSAEUSERS;
            errorcode["WSAEWOULDBLOCK"] = WSAEWOULDBLOCK;
            errorcode["WSANOTINITIALISED"] = WSANOTINITIALISED;
            errorcode["WSASYSNOTREADY"] = WSASYSNOTREADY;
            errorcode["WSAVERNOTSUPPORTED"] = WSAVERNOTSUPPORTED;
        }

        public static PythonDictionary errorcode;

        public const int E2BIG = 7;
        public const int EACCES = 13;
        public const int EADDRINUSE = 10048;
        public const int EADDRNOTAVAIL = 10049;
        public const int EAFNOSUPPORT = 10047;
        public const int EAGAIN = 11;
        public const int EALREADY = 10037;
        public const int EBADF = 9;
        public const int EBUSY = 16;
        public const int ECHILD = 10;
        public const int ECONNABORTED = 10053;
        public const int ECONNREFUSED = 10061;
        public const int ECONNRESET = 10054;
        public const int EDEADLK = 36;
        public const int EDEADLOCK = 36;
        public const int EDESTADDRREQ = 10039;
        public const int EDOM = 33;
        public const int EDQUOT = 10069;
        public const int EEXIST = 17;
        public const int EFAULT = 14;
        public const int EFBIG = 27;
        public const int EHOSTDOWN = 10064;
        public const int EHOSTUNREACH = 10065;
        public const int EILSEQ = 42;
        public const int EINPROGRESS = 10036;
        public const int EINTR = 4;
        public const int EINVAL = 22;
        public const int EIO = 5;
        public const int EISCONN = 10056;
        public const int EISDIR = 21;
        public const int ELOOP = 10062;
        public const int EMFILE = 24;
        public const int EMLINK = 31;
        public const int EMSGSIZE = 10040;
        public const int ENAMETOOLONG = 38;
        public const int ENETDOWN = 10050;
        public const int ENETRESET = 10052;
        public const int ENETUNREACH = 10051;
        public const int ENFILE = 23;
        public const int ENOBUFS = 10055;
        public const int ENODEV = 19;
        public const int ENOENT = 2;
        public const int ENOEXEC = 8;
        public const int ENOLCK = 39;
        public const int ENOMEM = 12;
        public const int ENOPROTOOPT = 10042;
        public const int ENOSPC = 28;
        public const int ENOSYS = 40;
        public const int ENOTCONN = 10057;
        public const int ENOTDIR = 20;
        public const int ENOTEMPTY = 41;
        public const int ENOTSOCK = 10038;
        public const int ENOTTY = 25;
        public const int ENXIO = 6;
        public const int EOPNOTSUPP = 10045;
        public const int EPERM = 1;
        public const int EPFNOSUPPORT = 10046;
        public const int EPIPE = 32;
        public const int EPROTONOSUPPORT = 10043;
        public const int EPROTOTYPE = 10041;
        public const int ERANGE = 34;
        public const int EREMOTE = 10071;
        public const int EROFS = 30;
        public const int ESHUTDOWN = 10058;
        public const int ESOCKTNOSUPPORT = 10044;
        public const int ESPIPE = 29;
        public const int ESRCH = 3;
        public const int ESTALE = 10070;
        public const int ETIMEDOUT = 10060;
        public const int ETOOMANYREFS = 10059;
        public const int EUSERS = 10068;
        public const int EWOULDBLOCK = 10035;
        public const int EXDEV = 18;
        public const int WSABASEERR = 10000;
        public const int WSAEACCES = 10013;
        public const int WSAEADDRINUSE = 10048;
        public const int WSAEADDRNOTAVAIL = 10049;
        public const int WSAEAFNOSUPPORT = 10047;
        public const int WSAEALREADY = 10037;
        public const int WSAEBADF = 10009;
        public const int WSAECONNABORTED = 10053;
        public const int WSAECONNREFUSED = 10061;
        public const int WSAECONNRESET = 10054;
        public const int WSAEDESTADDRREQ = 10039;
        public const int WSAEDISCON = 10101;
        public const int WSAEDQUOT = 10069;
        public const int WSAEFAULT = 10014;
        public const int WSAEHOSTDOWN = 10064;
        public const int WSAEHOSTUNREACH = 10065;
        public const int WSAEINPROGRESS = 10036;
        public const int WSAEINTR = 10004;
        public const int WSAEINVAL = 10022;
        public const int WSAEISCONN = 10056;
        public const int WSAELOOP = 10062;
        public const int WSAEMFILE = 10024;
        public const int WSAEMSGSIZE = 10040;
        public const int WSAENAMETOOLONG = 10063;
        public const int WSAENETDOWN = 10050;
        public const int WSAENETRESET = 10052;
        public const int WSAENETUNREACH = 10051;
        public const int WSAENOBUFS = 10055;
        public const int WSAENOPROTOOPT = 10042;
        public const int WSAENOTCONN = 10057;
        public const int WSAENOTEMPTY = 10066;
        public const int WSAENOTSOCK = 10038;
        public const int WSAEOPNOTSUPP = 10045;
        public const int WSAEPFNOSUPPORT = 10046;
        public const int WSAEPROCLIM = 10067;
        public const int WSAEPROTONOSUPPORT = 10043;
        public const int WSAEPROTOTYPE = 10041;
        public const int WSAEREMOTE = 10071;
        public const int WSAESHUTDOWN = 10058;
        public const int WSAESOCKTNOSUPPORT = 10044;
        public const int WSAESTALE = 10070;
        public const int WSAETIMEDOUT = 10060;
        public const int WSAETOOMANYREFS = 10059;
        public const int WSAEUSERS = 10068;
        public const int WSAEWOULDBLOCK = 10035;
        public const int WSANOTINITIALISED = 10093;
        public const int WSASYSNOTREADY = 10091;
        public const int WSAVERNOTSUPPORTED = 10092;

    }
}
