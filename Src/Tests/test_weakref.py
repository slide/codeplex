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

from lib.assert_util import *
skiptest("silverlight")

import _weakref
import gc

class NonCallableClass(object): pass

class CallableClass(object):
    def __call__(self, *args):
        return 42

def keep_alive(o): pass

def test_proxy_dir():
    # dir on a deletex proxy should return an empty list,
    # not throw.
    for cls in [NonCallableClass, CallableClass]:
        def run_test():
            a = cls()        
            b = _weakref.proxy(a)
            
            AreEqual(dir(a), dir(b))
            
            #Merlin 366014
            keep_alive(a)
            
            del(a)               
            
            return b
            
        prxy = run_test()
        if not is_silverlight:
            gc.collect()
            #This will fail if original object has not been garbage collected.
            AreEqual(dir(prxy), [])

def test_special_methods():    
    for cls in [NonCallableClass, CallableClass]:
        # calling repr should give us weakproxy's repr,
        # calling __repr__ should give us the underlying objects
        # repr
        a = cls()    
        b = _weakref.proxy(a)
        
        Assert(repr(b).startswith('<weakproxy at'))
        
        AreEqual(repr(a), b.__repr__())
        
        keep_alive(a)
        
    # calling a special method should work
    class strable(object):
            def __str__(self): return 'abc'

    a = strable()
    b = _weakref.proxy(a)
    AreEqual(str(b), 'abc')

    keep_alive(a)    


def test_type_call():
    def get_dead_weakref():
        class C: pass
        
        a = C()        
        x = _weakref.proxy(a)
        del(a)
        return x
        
    wr = get_dead_weakref()
    # Uncomment the next line after fixing merlin#243506
    # type(wr).__add__.__get__(wr, None) # no exception
    
    try:
        type(wr).__add__.__get__(wr, None)() # object is dead, should throw
    except: pass
    else: AssertUnreachable()
    
        
    # kwarg call
    class C: 
        def __add__(self, other):
            return "abc" + other
        
    a = C()        
    x = _weakref.proxy(a)
    
    if is_cli:      # cli accepts kw-args everywhere
        res = type(x).__add__.__get__(x, None)(other = 'xyz')
        AreEqual(res, "abcxyz")
    res = type(x).__add__.__get__(x, None)('xyz') # test success-case without keyword args
    AreEqual(res, "abcxyz")
    
    # calling non-existent method should raise attribute error
    try:
        type(x).__sub__.__get(x, None)('abc')
    except AttributeError: pass
    else: AssertUnreachable()

    if is_cli:      # cli accepts kw-args everywhere
        # calling non-existent method should raise attribute error (kw-arg version)
        try:
            type(x).__sub__.__get(x, None)(other='abc')
        except AttributeError: pass
        else: AssertUnreachable()

def test_slot_repr():
    class C: pass

    a = C()
    x = _weakref.proxy(a)
    AreEqual(repr(type(x).__add__), "<slot wrapper '__add__' of 'weakproxy' objects>")

def test_cp14632():
    '''
    Make sure '_weakref.proxy(...)==xyz' does not throw after '...'
    has been deleted.
    '''
    def helper_func():
        class C:
            def __eq__(self, *args, **kwargs): return True
    
        a = C()
        Assert(C()==3)
        x = _weakref.proxy(a)
        y = _weakref.proxy(a)
        AreEqual(x, y)
        AreEqual(x, a) #Just to keep 'a' alive up to this point.
        del a
        while dir(x)!=[]:
            gc.collect()
        
        return x, y
        
    x, y = helper_func()
    #CodePlex 14632
    #Assert(not x==3)
    AssertError(ReferenceError, lambda: x==y)


run_test(__name__)
