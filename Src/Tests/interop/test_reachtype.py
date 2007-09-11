#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################
    
import sys, nt

def environ_var(key): return [nt.environ[x] for x in nt.environ.keys() if x.lower() == key.lower()][0]

merlin_root = environ_var("MERLIN_ROOT")
sys.path.append(merlin_root + r"\Languages\IronPython\Tests")
sys.path.append(merlin_root + r"\Test\ClrAssembly\bin")

from lib.assert_util import *

skiptest("silverlight")

# in order to make peverify happy
from lib.file_util import filecopy, delete_files
filecopy(merlin_root + r"\Test\ClrAssembly\bin\loadtypesample.dll", sys.exec_prefix + r"\loadtypesample.dll")

import clr
clr.AddReference("loadtypesample")

keywords = ['pass', 'import', 'def', 'exec', 'except']
bultin_funcs = ['abs', 'type', 'file']
bultin_types = ['complex', 'StandardError']
bultin_constants = ['None', 'False']
modules = ['__builtin__', 'datetime', 'collections', 'site']

def test_interesting_names_as_namespace():
    # import
    for x in keywords + ['None']: 
        AssertError(SyntaxError, compile, "import %s" % x, "", "exec")
    
    import False; AreEqual(str(False.A), "<type 'A'>")    
    
    import abs; AreEqual(str(abs.A), "<type 'A'>")
    import type; AreEqual(str(type.A), "<type 'A'>")
    import file; AreEqual(str(file.A), "<type 'A'>")
    
    import complex; AreEqual(str(complex.A), "<type 'A'>")
    import StandardError; AreEqual(str(StandardError.A), "<type 'A'>")
    
    # !!! no way to get clr types which have the same name as builtin modules
    import __builtin__; AssertError(AttributeError, lambda: __builtin__.A)
    import datetime; AssertError(AttributeError, lambda: datetime.A)
    import collections; AssertError(AttributeError, lambda: collections.A)
    
    # __import__
    for x in keywords + bultin_constants + bultin_funcs + bultin_types:
        mod = __import__(x)
        AreEqual(str(mod.A), "<type 'A'>")
    
    for x in modules:
        mod = __import__(x)
        AssertError(AttributeError, lambda: mod.A)

def test_interesting_names_as_class_name():
    # from a import b
    for x in keywords: 
        AssertError(SyntaxError, compile, "from NSwInterestingClassName import %s" % x, "", "exec")

    # !!! special None    
    AssertError(SyntaxError, compile, "from NSwInterestingClassName import None", "", "exec")
    from NSwInterestingClassName import False; AreEqual(False.A, 10)
    
    from NSwInterestingClassName import abs; AreEqual(abs.A, 10)
    from NSwInterestingClassName import type; AreEqual(type.A, 10)
    from NSwInterestingClassName import file; AreEqual(file.A, 10)
    
    from NSwInterestingClassName import complex; AreEqual(complex.A, 10)
    from NSwInterestingClassName import StandardError; AreEqual(StandardError.A, 10)
    
    from NSwInterestingClassName import __builtin__; AreEqual(__builtin__.A, 10)
    from NSwInterestingClassName import datetime; AreEqual(datetime.A, 10)
    from NSwInterestingClassName import collections; AreEqual(collections.A, 10)
    
    # import a
    import NSwInterestingClassName
    for x in keywords: 
        AssertError(SyntaxError, compile, "NSwInterestingClassName.%s" % x, "", "exec")
        
    for x in bultin_constants + bultin_funcs + bultin_types + modules:
        x = eval("NSwInterestingClassName.%s" % x)
        AreEqual(x.A, 10)

def test_nothing_public():
    try: 
        import NothingPublic
        AssertUnreachable()
    except ImportError:
        pass
     
def test_generic_types():
    from NSwGeneric import G1, G2, G3

    AreEqual(G1.A, 10)
    AreEqual(G1[int, int].A, 20)
    #AreEqual(G1[G1, G1].A, 20)             # tracking bug 291326
    
    AssertError(SystemError, lambda: G2.A)
    AreEqual(G2[int].A, 30)
    AreEqual(G2[int, int].A, 40)
    
    AssertError(ValueError, lambda: G3[System.Exception])
    AreEqual(G3[int].A, 50)

def test_type_without_namespace():
    from PublicRefTypeWithoutNS import *    # warning expected
    AreEqual(Nested.A, 10)
    AreEqual(A, 20)
    AreEqual(SM(), 30)

    import PublicRefTypeWithoutNS
    AreEqual(PublicRefTypeWithoutNS.Nested.A, 10)
    AreEqual(PublicRefTypeWithoutNS.A, 20)
    AreEqual(PublicRefTypeWithoutNS.SM(), 30)
    
    AreEqual(PublicRefTypeWithoutNS.B, B)
    AreEqual(PublicRefTypeWithoutNS.IM, IM)
    
    AssertError(TypeError, IM)

    # internal type
    try:
        import InternalRefTypeWithoutNS
        AssertUnreachable()
    except ImportError:
        pass

def test_generic_type_without_namespace():
    import PublicValueTypeWithoutNS
    AssertError(SystemError, lambda: PublicValueTypeWithoutNS.A)
    AreEqual(60, PublicValueTypeWithoutNS[int].A)

def test_various_types():
    import NSwVarious
    AreEqual(dir(NSwVarious.NestedNS), ['A', 'B', 'C', 'D', 'E'])   # F should not be seen

import System
if '-X:SaveAssemblies' not in System.Environment.GetCommandLineArgs():
    # snippets.dll (if saved) has the reference to temp.dll, which is not saved.
    @runonly("orcas")
    def test_type_from_reflection_emit():
        
        sr = System.Reflection
        sre = System.Reflection.Emit
        array = System.Array
        cab = array[sre.CustomAttributeBuilder]([sre.CustomAttributeBuilder(clr.GetClrType(System.Security.SecurityTransparentAttribute).GetConstructor(System.Type.EmptyTypes), array[object]([]))])
        ab = System.AppDomain.CurrentDomain.DefineDynamicAssembly(sr.AssemblyName("temp"), sre.AssemblyBuilderAccess.RunAndSave, "temp", None, None, None, None, True, cab)  # tracking: 291888

        mb = ab.DefineDynamicModule("temp", "temp.dll")
        tb = mb.DefineType("EmittedNS.EmittedType", sr.TypeAttributes.Public)
        tb.CreateType()
            
        clr.AddReference(ab)
        import EmittedNS
        EmittedNS.EmittedType()
    
def test_type_forwarded():
    clr.AddReference("typeforwarder")
    from NSwForwardee import Foo, Bar        #!!!
    AreEqual(Foo.A, 120)
    AreEqual(Bar.A, -120)
    
    import NSwForwardee
    AreEqual(NSwForwardee.Foo.A, 120)
    AreEqual(NSwForwardee.Bar.A, -120)

def test_type_forward2():    
    clr.AddReference("typeforwarder2")
    from NSwForwardee2 import *      
    Assert('Foo_SPECIAL' not in dir())      # !!!
    Assert('Bar_SPECIAL' in dir())
    
    import NSwForwardee2
    AreEqual(NSwForwardee2.Foo_SPECIAL.A, 620)
    AreEqual(NSwForwardee2.Bar_SPECIAL.A, 64)
    
def test_type_forward3():    
    clr.AddReference("typeforwarder3")
    #import NSwForwardee3                   # TRACKING BUG: 291692
    #AreEqual(NSwForwardee3.Foo.A, 210)
    
def test_type_causing_load_exception():
    clr.AddReference("loadexception")
    from PossibleLoadException import A, C
    AreEqual(A.F, 10)
    AreEqual(C.F, 30)

    B = 10    
    try:
        from PossibleLoadException import B
        AssertUnreachable()
    except ImportError: 
        pass

    import PossibleLoadException
    AreEqual(PossibleLoadException.A.F, 10)
    AssertError(AttributeError, lambda: PossibleLoadException.B)
    AreEqual(PossibleLoadException.C.F, 30)
    AreEqual(B, 10)

run_test(__name__)    

# will not succeed in the SaveAssemblies mode
delete_files(sys.exec_prefix + r"\loadtypesample.dll")
