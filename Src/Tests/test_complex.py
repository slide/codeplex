#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################

from lib.assert_util import *
from lib.type_util import *

def test_from_string():
    # complex from string: negative 
    # - space related
    l = ['1.2', '.3', '4e3', '.3e-4', "0.031"]

    for x in l:   
        for y in l:
            AssertError(ValueError, complex, "%s +%sj" % (x, y))
            AssertError(ValueError, complex, "%s+ %sj" % (x, y))
            AssertError(ValueError, complex, "%s - %sj" % (x, y))
            AssertError(ValueError, complex, "%s-  %sj" % (x, y))
            AssertError(ValueError, complex, "%s-\t%sj" % (x, y))
            AssertError(ValueError, complex, "%sj+%sj" % (x, y))
            AreEqual(complex("   %s+%sj" % (x, y)), complex(" %s+%sj  " % (x, y)))


def test_misc():
    AreEqual(mycomplex(), complex())
    a = mycomplex(1)
    b = mycomplex(1,0)
    c = complex(1)
    d = complex(1,0)

    for x in [a,b,c,d]:
        for y in [a,b,c,d]:
            AreEqual(x,y)

    AreEqual(a ** 2, a)
    AreEqual(a-complex(), a)
    AreEqual(a+complex(), a)
    AreEqual(complex()/a, complex())
    AreEqual(complex()*a, complex())
    AreEqual(complex()%a, complex())
    AreEqual(complex() // a, complex())

    Assert(complex(2) == complex(2, 0))
    
def test_inherit():
    class mycomplex(complex): pass
    
    a = mycomplex(2+1j)
    AreEqual(a.real, 2)
    AreEqual(a.imag, 1)


run_test(__name__)
