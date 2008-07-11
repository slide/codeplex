#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Public License. A
# copy of the license can be found in the License.html file at the root of this distribution. If
# you cannot locate the  Microsoft Public License, please send an email to
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

"module doc"

from lib.assert_util import *

@skip('interactive', "multiple_execute")
def test_sanity():
    ## module
    global __doc__
    
    AreEqual(__doc__, "module doc")
    __doc__ = "new module doc"
    AreEqual(__doc__, "new module doc")

    ## builtin
    Assert(min.__doc__ <> None)
    
    AreEqual(abs.__doc__, "abs(number) -> number\n\nReturn the absolute value of the argument.")
    AreEqual(int.__add__.__doc__, "x.__add__(y) <==> x+y")


def f_1():
    "f 1 doc"
    return __doc__

def f_2():
    __doc__ = "f 2 doc"
    return __doc__

def f_3():
    "f 3 doc"
    __doc__ = "new f 3 doc"
    return __doc__

def f_4():
    return __doc__

class c_1:
    "c 1 doc"
    AreEqual(__doc__, "c 1 doc")

class c_2:
    "c 2 doc"
    AreEqual(__doc__, "c 2 doc")

class c_3:
    "c 3 doc"
    AreEqual(__doc__, "c 3 doc")
    __doc__ = "c 3 doc 2"
    AreEqual(__doc__, "c 3 doc 2")

class c_4:
    __doc__ = "c 4 doc"
    AreEqual(__doc__, "c 4 doc")

class n_1(object):
    "n 1 doc"
    AreEqual(__doc__, "n 1 doc")

class n_2(object):
    "n 2 doc"
    AreEqual(__doc__, "n 2 doc")

class n_3(object):
    "n 3 doc"
    AreEqual(__doc__, "n 3 doc")
    __doc__ = "n 3 doc 2"
    AreEqual(__doc__, "n 3 doc 2")

class n_4(object):
    __doc__ = "n 4 doc"
    AreEqual(__doc__, "n 4 doc")

class d:
    "d doc 1"
    AreEqual(__doc__, "d doc 1")

    def m_1(self):
        "m 1 doc"
        return __doc__

    AreEqual(m_1.__doc__, "m 1 doc")
    AreEqual(__doc__, "d doc 1")
    __doc__ = "d doc 2"
    AreEqual(__doc__, "d doc 2")
    AreEqual(m_1.__doc__, "m 1 doc")

    def m_2(self):
        __doc__ = "m 2 doc"
        return __doc__

    AreEqual(m_2.__doc__, None)
    AreEqual(__doc__, "d doc 2")
    __doc__ = "d doc 3"
    AreEqual(__doc__, "d doc 3")
    AreEqual(m_2.__doc__, None)

    def m_3(self):
        "m 3 doc"
        __doc__ = "new m 3 doc"
        return __doc__

    AreEqual(m_3.__doc__, "m 3 doc")
    AreEqual(__doc__, "d doc 3")
    __doc__ = "d doc 4"
    AreEqual(__doc__, "d doc 4")
    AreEqual(m_3.__doc__, "m 3 doc")

    def m_4(self):
        return __doc__

    AreEqual(m_4.__doc__, None)
    AreEqual(__doc__, "d doc 4")
    __doc__ = "d doc 5"
    AreEqual(__doc__, "d doc 5")
    AreEqual(m_4.__doc__, None)


def test_func_meth_class():
    AreEqual(f_1.__doc__, "f 1 doc")
    AreEqual(f_2.__doc__, None)
    AreEqual(f_3.__doc__, "f 3 doc")
    AreEqual(f_4.__doc__, None)

    AreEqual(c_1.__doc__, "c 1 doc")
    AreEqual(c_2.__doc__, "c 2 doc")
    AreEqual(c_3.__doc__, "c 3 doc 2")
    AreEqual(c_4.__doc__, "c 4 doc")

    AreEqual(n_1.__doc__, "n 1 doc")
    AreEqual(n_2.__doc__, "n 2 doc")
    AreEqual(n_3.__doc__, "n 3 doc 2")
    AreEqual(n_4.__doc__, "n 4 doc")

    AreEqual(d.__doc__, "d doc 5")
    AreEqual(d.m_1.__doc__, "m 1 doc")
    AreEqual(d.m_2.__doc__, None)
    AreEqual(d.m_3.__doc__, "m 3 doc")
    AreEqual(d.m_4.__doc__, None)

    dd = d()
    for x in (f_1, f_2, f_3, f_4,
                c_1, c_2, c_3, c_4,
                n_1, n_2, n_3, n_4,
                dd.m_1, dd.m_2, dd.m_3, dd.m_4):
        x()

@skip('win32')
def test_clr_doc():
    import System
    Assert(System.Collections.Generic.List.__doc__.find("List[T]()") != -1)
    Assert(System.Collections.Generic.List.__doc__.find("IEnumerable[T] collection") != -1)

    # static (bool, float) TryParse(str s)
    Assert(System.Double.TryParse.__doc__.index('(bool, float)') >= 0)
    Assert(System.Double.TryParse.__doc__.index('(str s)') >= 0)

def test_none():
    AreEqual(None.__doc__, None)
    
    
def test_types():
    AreEqual(Ellipsis.__doc__, None)
    AreEqual(NotImplemented.__doc__, None)

def test_builtin_nones():
    for x in [Ellipsis, None, NotImplemented, ]:
        Assert(x.__doc__==None, str(x) + ".__doc__ != None")

#Needs CPython's site.py for this to work under IP.
#See CodePlex 10823 for details.
@skip("cli", "silverlight")
def test_builtin_nones_cpy_site():
    for x in [exit, quit, ]:
        Assert(x.__doc__==None, str(x) + ".__doc__ != None")

def test_class_doc():
    # sanity, you can assign to __doc__
    class x(object): pass
    
    class y(object):
        __slots__ = '__doc__'
    
    class z(object):
        __slots__ = '__dict__'

    for t in (x, y, z):
        a = t()
        a.__doc__ = 'Hello World'    
        AreEqual(a.__doc__, 'Hello World')
    
    
    class x(object): __slots__ = []
    
    def f(a): a.__doc__ = 'abc'
    AssertError(AttributeError, f, x())
    AssertError(AttributeError, f, object())

run_test(__name__)
