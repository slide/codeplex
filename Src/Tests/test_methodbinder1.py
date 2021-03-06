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

#
# PART 1. how IronPython choose the CLI method, treat parameters WHEN NO OVERLOADS PRESENT
#

from lib.assert_util import *
from lib.type_util import *

load_iron_python_test()
from IronPythonTest.BinderTest import *

myint1,     myint2      = myint(20),    myint(-20)
mylong1,    mylong2     = mylong(3),    mylong(-4)
myfloat1,   myfloat2    = myfloat(4.5), myfloat(-4.5)
mycomplex1              = mycomplex(3)

funcs = '''
M100   M201   M202   M203   M204   M205   M301   M302   M303   M304
M310   M311   M312   M313   M320   M321   M400   M401   M402   M403
M404   M410   M411   M450   M451   
M500   M510   M600   M610   M611   M620   M630   
M650   M651   M652   M653   
M680   M700   M701   
M710   M715  
'''.split()

args  = '''
NoArg  Int32  Double BigInt Bool   String SByte  Int16  Int64  Single
Byte   UInt16 UInt32 UInt64 Char   Decml  Object I      C1     C2
S1     A      C6     E1     E2     
ArrInt32  ArrI   ParamArrInt32  ParamArrI       ParamArrS   Int32ParamArrInt32  IParamArrI  
IListInt  Array  IEnumerableInt IEnumeratorInt
NullableInt RefInt32  OutInt32 
DefValInt32 Int32DefValInt32
'''.split()

arg2func = dict(zip(args, funcs))
func2arg = dict(zip(funcs, args))

TypeE = TypeError
OverF = OverflowError

def _get_funcs(args): return [arg2func[x] for x in args.split()]
def _self_defined_method(name): return len(name) == 4 and name[0] == "M"

def _my_call(func, arg):
    if isinstance(arg, tuple): 
        l = len(arg)
        # by purpose 
        if l == 0: func()
        elif l == 1: func(arg[0])
        elif l == 2: func(arg[0], arg[1])
        elif l == 3: func(arg[0], arg[1], arg[2])
        elif l == 4: func(arg[0], arg[1], arg[2], arg[3])
        elif l == 5: func(arg[0], arg[1], arg[2], arg[3], arg[4])
        elif l == 6: func(arg[0], arg[1], arg[2], arg[3], arg[4], arg[5])
        else: func(*arg)
    else:
        func(arg)
    
def _helper(func, positiveArgs, flagValue, negativeArgs, exceptType):
    for arg in positiveArgs:
        try:
            _my_call(func, arg)
        except Exception, e:
            Fail("unexpected exception %s when calling %s with %s\n%s" % (e, func, arg, func.__doc__))
        else:
            AreEqual(Flag.Value, flagValue)
            Flag.Value = -188
    
    for arg in negativeArgs:
        try:
            _my_call(func, arg)
        except Exception, e:
            if not isinstance(e, exceptType):
                Fail("expected '%s', but got '%s' when calling %s with %s\n%s" % (exceptType, e, func, arg, func.__doc__))
        else:
            Fail("expected exception (but didn't get one) when calling func %s on args %s\n%s" % (func, arg, func.__doc__))

def test_this_matrix():
    '''
    This will test the full matrix.
    To print the matrix, enable the following flag
    '''
    print_the_matrix = False


    funcnames =     "M201   M680   M202   M203   M204   M205   M301   M302   M303   M304   M310   M311   M312   M313   M320   M321   M400".split()
    matrix = (   
####                 M201   M680   M202   M203   M204   M205   M301   M302   M303   M304   M310   M311   M312   M313   M320   M321   M400
####                 int    int?   double bigint bool   str    sbyte  i16    i64    single byte   ui16   ui32   ui64   char   decm   obj 
(        "SByteMax", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(         "ByteMax", True,  True,  True,  True,  True,  TypeE, OverF, True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(        "Int16Max", True,  True,  True,  True,  True,  TypeE, OverF, True,  True,  True,  OverF, True,  True,  True,  TypeE, True,  True,  ),
(       "UInt16Max", True,  True,  True,  True,  True,  TypeE, OverF, OverF, True,  True,  OverF, True,  True,  True,  TypeE, True,  True,  ),
(          "intMax", True,  True,  True,  True,  True,  TypeE, OverF, OverF, True,  True,  OverF, OverF, True,  True,  TypeE, True,  True,  ),
(       "UInt32Max", OverF, OverF, True,  True,  True,  TypeE, OverF, OverF, True,  True,  OverF, OverF, True,  True,  TypeE, True,  True,  ),
(        "Int64Max", OverF, OverF, True,  True,  True,  TypeE, OverF, OverF, True,  True,  OverF, OverF, OverF, True,  TypeE, True,  True,  ),
(       "UInt64Max", OverF, OverF, True,  True,  True,  TypeE, OverF, OverF, OverF, True,  OverF, OverF, OverF, True,  TypeE, True,  True,  ),
(      "decimalMax", OverF, OverF, True,  True,  True,  TypeE, OverF, OverF, OverF, True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(       "SingleMax", TypeE, TypeE, True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, OverF, True,  ),
(        "floatMax", OverF, OverF, True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, OverF, TypeE, TypeE, TypeE, TypeE, TypeE, OverF, True,  ),
####                 M201   M680   M202   M203   M204   M205   M301   M302   M303   M304   M310   M311   M312   M313   M320   M321   M400
####                 int    int?   double bigint bool   str    sbyte  i16    i64    single byte   ui16   ui32   ui64   char   decm   obj 
(        "SByteMin", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(         "ByteMin", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(        "Int16Min", True,  True,  True,  True,  True,  TypeE, OverF, True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(       "UInt16Min", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(          "intMin", True,  True,  True,  True,  True,  TypeE, OverF, OverF, True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(       "UInt32Min", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(        "Int64Min", OverF, OverF, True,  True,  True,  TypeE, OverF, OverF, True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(       "UInt64Min", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(      "decimalMin", OverF, OverF, True , True,  True,  TypeE, OverF, OverF, OverF, True,  OverF, OverF, OverF, OverF, TypeE, True , True,  ), 
(       "SingleMin", TypeE, TypeE, True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, OverF, True,  ),
(        "floatMin", OverF, OverF, True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, OverF, TypeE, TypeE, TypeE, TypeE, TypeE, OverF, True,  ),
####                 M201   M680   M202   M203   M204   M205   M301   M302   M303   M304   M310   M311   M312   M313   M320   M321   M400
####                 int    int?   double bigint bool   str    sbyte  i16    i64    single byte   ui16   ui32   ui64   char   decm   obj 
(    "SBytePlusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(     "BytePlusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(    "Int16PlusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(   "UInt16PlusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(      "intPlusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(            myint1, True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(   "UInt32PlusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(    "Int64PlusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(   "UInt64PlusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(  "decimalPlusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(   "SinglePlusOne", TypeE, TypeE, True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
(    "floatPlusOne", True,  True,  True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
(          myfloat1, True,  True,  True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
####                 M201   M680   M202   M203   M204   M205   M301   M302   M303   M304   M310   M311   M312   M313   M320   M321   M400
####                 int    int?   double bigint bool   str    sbyte  i16    i64    single byte   ui16   ui32   ui64   char   decm   obj 
(   "SByteMinusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(   "Int16MinusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(     "intMinusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(            myint2, True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(   "Int64MinusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
( "decimalMinusOne", True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
(  "SingleMinusOne", TypeE, TypeE, True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
(   "floatMinusOne", True,  True,  True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
(          myfloat2, True,  True,  True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
##################################################   pass in bool   #########################################################
####                 M201   M680   M202   M203   M204   M205   M301   M302   M303   M304   M310   M311   M312   M313   M320   M321   M400
####                 int    int?   double bigint bool   str    sbyte  i16    i64    single byte   ui16   ui32   ui64   char   decm   obj 
(              True, True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(             False, True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
##################################################  pass in BigInt #########################################################
####                 int    int?   double bigint bool   str    sbyte  i16    i64    single byte   ui16   ui32   ui64   char   decm   obj 
####                 M201   M680   M202   M203   M204   M205   M301   M302   M303   M304   M310   M311   M312   M313   M320   M321   M400
(               10L, True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(              -10L, True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
( 1234567890123456L, OverF, OverF, True , True,  True,  TypeE, OverF, OverF, True,  True,  OverF, OverF, OverF, True,  TypeE, True,  True,  ),        
(           mylong1, True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  True,  True,  True,  True,  TypeE, True,  True,  ),
(           mylong2, True,  True,  True,  True,  True,  TypeE, True,  True,  True,  True,  OverF, OverF, OverF, OverF, TypeE, True,  True,  ),
##################################################  pass in Complex #########################################################
####                 M201   M680   M202   M203   M204   M205   M301   M302   M303   M304   M310   M311   M312   M313   M320   M321   M400
####                 int    int?   double bigint bool   str    sbyte  i16    i64    single byte   ui16   ui32   ui64   char   decm   obj 
(            (3+0j), TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, True,  ),
(            (3+1j), TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, True,  ),
(        mycomplex1, TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, True,  ),    
##################################################  pass in char    #########################################################
####                 M201   M680   M202   M203   M204   M205   M301   M302   M303   M304   M310   M311   M312   M313   M320   M321   M400
####                          int    int?   double bigint bool   str    sbyte  i16    i64    single byte   ui16   ui32   ui64   char   decm   obj 
(     System.Char.Parse('A'), TypeE, TypeE, TypeE, TypeE, True,  True,  TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, TypeE, True,  TypeE, True,  ),
##################################################  pass in float   #########################################################
####   single/double becomes Int32, but this does not apply to other primitive types
####                          int    int?   double bigint bool   str    sbyte  i16    i64    single byte   ui16   ui32   ui64   char   decm   obj 
(System.Single.Parse("8.01"), TypeE, TypeE, True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
(System.Single.Parse("-8.1"), TypeE, TypeE, True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
(System.Double.Parse("10.2"), True,  True,  True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
(System.Double.Parse("-1.8"), True,  True,  True,  TypeE, True,  TypeE, TypeE, TypeE, TypeE, True,  TypeE, TypeE, TypeE, TypeE, TypeE, True,  True,  ),
    )
    
    for scenario in matrix: 
        if isinstance(scenario[0], str):    
            value = clr_numbers[scenario[0]]
            if print_the_matrix: print '(%18s,' % ('"'+ scenario[0] +'"'),
        else:                               
            value = scenario[0]
            if print_the_matrix: print '(%18s,' % value ,
        
        for i in range(len(funcnames)):
            funcname = funcnames[i]
            func = getattr(target, funcname)
            
            if print_the_matrix:
                try:
                    func(value)
                    print "True, ",
                except TypeError:
                    print "TypeE,",
                except OverflowError:
                    print "OverF,",
                print "),"
            else:
                try:
                    func(value)
                except Exception,e:
                    if scenario[i+1] not in [TypeE, OverF]:
                        Fail("unexpected exception %s, when func %s on arg %s (%s)\n%s" % (e, funcname, scenario[0], type(value), func.__doc__))
                    if isinstance(e, scenario[i+1]): pass
                    else: Fail("expect %s, but got %s when func %s on arg %s (%s)\n%s" % (scenario[i+1], e, funcname, scenario[0], type(value), func.__doc__))
                else:
                    if scenario[i+1] in [TypeE, OverF]:
                        Fail("expect %s, but got none when func %s on arg %s (%s)\n%s" % (scenario[i+1], funcname, scenario[0], type(value), func.__doc__))

                    left = Flag.Value ; Flag.Value = -99           # reset

                    right = int(funcname[1:])
                    if left != right: 
                        Fail("left %s != right %s when func %s on arg %s (%s)\n%s" % (left, right, funcname, scenario[0], type(value), func.__doc__))

    # these funcs should behavior same as M201(Int32)  
    # should have NullableInt too ?
    for funcname in _get_funcs('RefInt32   ParamArrInt32   Int32ParamArrInt32'): 
        for scenario in matrix: 
            if isinstance(scenario[0], str): value = clr_numbers[scenario[0]]
            else: value = scenario[0]

            func = getattr(target, funcname)
            if scenario[1] not in [TypeE, OverF]:
                func(value)
                left = Flag.Value
                right = int(funcname[1:])
                if left != right: 
                    Fail("left %s != right %s when func %s on arg %s" % (left, right, funcname, scenario[0]))
                Flag.Value = -99           # reset 
            else: 
                try:   func(value)
                except scenario[1]: pass   # 1 is M201
                else:  Fail("expect %s, but got none when func %s on arg %s" % (scenario[1], funcname, scenario[0]))

def test_char_string_asked():    
    # char asked
    _helper(target.M320, ['a', System.Char.MaxValue, System.Char.MinValue, 'abc'[2]], 320, ['abc', ('a  b')], TypeError)
    # string asked
    _helper(target.M205, ['a', System.Char.MaxValue, System.Char.MinValue, 'abc'[2], 'abc', 'a b' ], 205, [('a', 'b'), 23, ], TypeError)
    
def test_pass_extensible_types():
    # number covered by that matrix
    # string or char
    mystr1, mystr2 = mystr('a'), mystr('abc')
    _helper(target.M205, [mystr1, mystr2, ], 205, [], TypeError)  # String
    _helper(target.M320, [mystr1, ], 320, [mystr2, ], TypeError)  # Char

# check the bool conversion result    
def test_bool_asked():
    for arg in ['a', 3, object(), True]:
        target.M204(arg)
        Assert(Flag.BValue, "argument is %s" % arg)
        Flag.BValue = False
    for arg in [0, System.Byte.Parse('0'), System.UInt64.Parse('0'), 0.0, 0L, False, None, tuple(), list()]:
        target.M204(arg)
        Assert(not Flag.BValue, "argument is %s" % (arg,))
        Flag.BValue = True    

def test_user_defined_conversion():
    class CP1: 
        def __int__(self): return 100
    class CP2(object): 
        def __int__(self): return 99
    class CP3: pass        
    cp1, cp2, cp3 = CP1(), CP2(), CP3()

    ### 1. not work for Nullable<Int32> required (?)
    ### 2. (out int): should pass in nothing
    ###      int  params int int?          ref int   defVal  int+defVal
    works = 'M201 M600       M680     M620   M700      M710    M715'
    for fn in works.split():
        _helper(getattr(target, fn), [cp1, cp2, ], int(fn[1:]), [cp3, ], TypeError)
    
    for fn in dir(target):
    ###                                                     bool  obj
        if _self_defined_method(fn) and fn not in (works + 'M204  M400 '): 
            _helper(getattr(target, fn), [], 0, [cp1, cp2, cp3, ], TypeError)

def test_pass_in_derived_python_types():
    class CP1(I): pass
    class CP2(C1): pass
    class CP3(C2): pass
    class CP4(C6, I): pass    
    cp1, cp2, cp3, cp4 = CP1(), CP2(), CP3(), CP4()

    # I asked
    _helper(target.M401, [C1(), C2(), S1(), cp1, cp2, cp3, cp4,], 401,[C3(), object()], TypeError)
    # C2 asked
    _helper(target.M403, [C2(), cp3, ], 403, [C3(), object(), C1(), cp1, cp2, cp4, ], TypeError)
    
    class CP1(A): pass
    class CP2(C6): pass
    cp1, cp2 = CP1(), CP2()
    
    # A asked
    _helper(target.M410, [C6(), cp1, cp2, cp4,], 410, [C3(), object(), C1(), cp3, ], TypeError)
    # C6 asked
    _helper(target.M411, [C6(), cp2, cp4, ], 411, [C3(), object(), C1(), cp1, cp3,], TypeError)
    
def test_nullable_int():
    _helper(target.M680, [None, 100, 100L, System.Byte.MaxValue, System.UInt32.MinValue, myint1, mylong2, 3.6, ], 680, [(), 3+1j], TypeError)
    
def test_out_int():
    _helper(target.M701, [], 701, [1, 10L, None, System.Byte.Parse('3')], TypeError)    # not allow to pass in anything
    
def test_collections():
    arrayInt = array_int((10, 20))
    tupleInt = ((10, 20), )
    listInt  = ([10, 20], )
    tupleBool = ((True, False, True, True, False), )
    tupleLong1, tupleLong2  = ((10L, 20L), ), ((System.Int64.MaxValue, System.Int32.MaxValue * 2),)    
    arrayByte = array_byte((10, 20))
    arrayObj = array_object(['str', 10])
    
    # IList<int>
    _helper(target.M650, [arrayInt, tupleInt, listInt, arrayObj, tupleLong1, tupleLong2, ], 650, [arrayByte, ], TypeError)
    # arrayObj, tupleLong1, tupleLong2 : conversion happens late

    # Array
    _helper(target.M651, [arrayInt, arrayObj, arrayByte, ], 651, [listInt, tupleInt, tupleLong1, tupleLong2, ], TypeError)
    
    # IEnumerable[int]
    _helper(target.M652, [arrayInt, arrayObj, arrayByte, listInt, tupleInt, tupleLong1, tupleLong2, ], 652, [], TypeError)
    
    # IEnumerator[int]
    _helper(target.M653, [], 653, [arrayInt, arrayObj, arrayByte, listInt, tupleInt, tupleLong1, tupleLong2, ], TypeError)
    
    # Int32[]
    _helper(target.M500, [arrayInt, tupleInt, tupleLong1, tupleBool, ], 500, [listInt, arrayByte, arrayObj, ], TypeError)
    _helper(target.M500, [], 500, [tupleLong2, ], OverflowError)
    # params Int32[]
    _helper(target.M600, [arrayInt, tupleInt, tupleLong1, tupleBool, ], 600, [listInt, arrayByte, arrayObj, ], TypeError)
    _helper(target.M600, [], 600, [tupleLong2, ], OverflowError)
    
    # Int32, params Int32[]
    _helper(target.M620, [(10, 10), (10L, 10), (10L, 10L), (10, 10L), (10, arrayInt), (10, (10, 20)), ], 620, [(10, [10, 20]), ], TypeError)
    _helper(target.M620, [], 620, [(10, 123456789101234L), ], OverflowError)
    
    arrayI1 = System.Array[I]( (C1(), C2()) )
    arrayI2 = System.Array[I]( () )
    arrayObj3 = System.Array[object]( (C1(), C2()) )  
    tupleI = ((C1(), C2()),)
    listI =  ([C1(), C2()],)
    _helper(target.M510, [arrayI1, arrayI2, tupleI, ], 510, [arrayObj3, listI, ], TypeError)     # I[]
    _helper(target.M610, [arrayI1, arrayI2, tupleI, ], 610, [arrayObj3, listI, ], TypeError)     # params I[]

def test_no_arg_asked():
    # no args asked
    _helper(target.M100, [()], 100, [2, None, (2, None)], TypeError)    

def test_enum():
    # E1 asked
    _helper(target.M450, [E1.A, ], 450, [10, E2.A], TypeError)
    # E2: ushort asked
    _helper(target.M451, [E2.A, ], 451, [10, E1.A, System.UInt16.Parse("3")], TypeError)

def _repeat_with_one_arg(goodStr, getArg):    
    passSet = _get_funcs(goodStr)
    skipSet = []

    for fn in passSet:
        if fn in skipSet: continue
        
        arg = getArg()
        getattr(target, fn)(arg)
        left = Flag.Value
        right = int(fn[1:])
        if left != right: 
            Fail("left %s != right %s when func %s on arg %s" % (left, right, fn, arg))
    
    for fn in dir(target):
        if _self_defined_method(fn) and (fn not in passSet) and (fn not in skipSet):
            arg = getArg()            
            try:   getattr(target, fn)(arg)
            except TypeError : pass
            else:  Fail("expect TypeError, but got none when func %s on arg %s" % (fn, arg))

def test_pass_in_none():
    _repeat_with_one_arg('''
BigInt Bool String Object I C1 C2 A C6 
ArrInt32 ArrI ParamArrInt32 ParamArrI ParamArrS IParamArrI 
IListInt Array IEnumerableInt IEnumeratorInt NullableInt
''', lambda : None)

def test_pass_in_clrReference():
    import clr        
    _repeat_with_one_arg('Object RefInt32  OutInt32', lambda : clr.Reference[int]())
    _repeat_with_one_arg('Object OutInt32', lambda : clr.Reference[object](None))
    _repeat_with_one_arg('Object RefInt32  OutInt32', lambda : clr.Reference[int](10))
    _repeat_with_one_arg('Object ', lambda : clr.Reference[float](123.123))
    _repeat_with_one_arg('Object', lambda : clr.Reference[type](str)) # ref.Value = (type)

def test_pass_in_nothing():
    passSet = _get_funcs('NoArg ParamArrInt32 ParamArrS ParamArrI OutInt32 DefValInt32')
    skipSet = [ ]  # be empty before release
    
    for fn in passSet:
        if fn in skipSet: continue
        
        getattr(target, fn)()
        left = Flag.Value
        right = int(fn[1:])
        if left != right: 
            Fail("left %s != right %s when func %s on arg Nothing" % (left, right, fn))
    
    for fn in dir(target):
        if _self_defined_method(fn) and (fn not in passSet) and (fn not in skipSet):
            try:   getattr(target, fn)()
            except TypeError : pass
            else:  Fail("expect TypeError, but got none when func %s on arg Nothing" % fn)
    
def test_other_concern():
    target = COtherConcern()
    
    # static void M100()
    target.M100()
    AreEqual(Flag.Value, 100); Flag.Value = 99
    COtherConcern.M100()
    AreEqual(Flag.Value, 100); Flag.Value = 99
    AssertError(TypeError, target.M100, target)
    AssertError(TypeError, COtherConcern.M100, target)
    
    # static void M101(COtherConcern arg)
    target.M101(target)
    AreEqual(Flag.Value, 101); Flag.Value = 99
    COtherConcern.M101(target)
    AreEqual(Flag.Value, 101); Flag.Value = 99
    AssertError(TypeError, target.M101)
    AssertError(TypeError, COtherConcern.M101)
    
    # void M102(COtherConcern arg)
    target.M102(target)
    AreEqual(Flag.Value, 102); Flag.Value = 99
    COtherConcern.M102(target, target)
    AreEqual(Flag.Value, 102); Flag.Value = 99
    AssertError(TypeError, target.M102)
    AssertError(TypeError, COtherConcern.M102, target)
    
    # generic method
    target.M200[int](100)
    AreEqual(Flag.Value, 200); Flag.Value = 99
    target.M200[int](100.1234)
    AreEqual(Flag.Value, 200); Flag.Value = 99
    target.M200[long](100)
    AreEqual(Flag.Value, 200); Flag.Value = 99
    AssertError(OverflowError, target.M200[System.Byte], 300)
    AssertError(OverflowError, target.M200[int], 12345678901234)
    
    # what does means when passing in None 
    target.M300(None)
    AreEqual(Flag.Value, 300); Flag.Value = 99
    AreEqual(Flag.BValue, True)
    target.M300(C1())
    AreEqual(Flag.BValue, False)
    
    # void M400(ref Int32 arg1, out Int32 arg2, Int32 arg3) etc...
    AreEqual(target.M400(1, 100), (100, 100))
    AreEqual(target.M401(1, 100), (100, 100))
    AreEqual(target.M402(100, 1), (100, 100))
    
    # default Value
    target.M450()
    AreEqual(Flag.Value, 80); Flag.Value = 99
    
    # 8 args
    target.M500(1,2,3,4,5,6,7,8)
    AreEqual(Flag.Value, 500)
    AssertError(TypeError, target.M500)
    AssertError(TypeError, target.M500, 1)
    AssertError(TypeError, target.M500, 1,2,3,4,5,6,7,8,9)
    
    # IDictionary
    for x in [ {1:1}, {"str": 3} ]:
        target.M550(x)
        AreEqual(Flag.Value, 550); Flag.Value = 99
    AssertError(TypeError, target.M550, [1, 2])
    
    # not supported
    for fn in (target.M600, target.M601, target.M602):
        for l in ( {1:'a'}, [1,2], (1,2) ):
            AssertError(TypeError, fn, l)
            
    # delegate 
    def f(x): return x * x
    AssertError(TypeError, target.M700, f) 

    from IronPythonTest import IntIntDelegate
    for x in (lambda x: x, lambda x: x*2, f):
        target.M700(IntIntDelegate(x))
        AreEqual(Flag.Value, x(10)); Flag.Value = 99
    
    target.M701(lambda x: x*2)
    AreEqual(Flag.Value, 20); Flag.Value = 99
    AssertError(TypeError, target.M701, lambda : 10)
    
    # keywords
    x = target.M800(arg1 = 100, arg2 = 200L, arg3 = 'this'); AreEqual(x, 'THIS')
    x = target.M800(arg3 = 'Python', arg1 = 100, arg2 = 200L); AreEqual(x, 'PYTHON')
    x = target.M800(100, arg3 = 'iron', arg2 = C1()); AreEqual(x, 'IRON')
    
    try: target.M800(100, 'Yes', arg2 = C1())
    except TypeError: pass
    else: Fail("expect: got multiple values for keyword argument arg2")
    
    # more ref/out sanity check
    import clr
    def f1(): return clr.Reference[object]()
    def f2(): return clr.Reference[int](10)
    def f3(): return clr.Reference[S1](S1())
    def f4(): return clr.Reference[C1](C2()) # C2 inherits C1

    for (f, a, b, c, d) in [ 
        ('M850', False, False, True, False), 
        ('M851', False, False, False, True), 
        ('M852', True, False, True, False), 
        ('M853', True, False, False, True), 
    ]:
        expect = (f in 'M850 M852') and S1 or C1
        func = getattr(target, f)
        
        for i in range(4): 
            ref = (f1, f2, f3, f4)[i]()
            if (a,b,c,d)[i]:
                func(ref); AreEqual(type(ref.Value), expect)
            else: 
                AssertError(TypeError, func, ref)

    # call 854
    AssertError(TypeError, target.M854, clr.Reference[object]())
    AssertError(TypeError, target.M854, clr.Reference[int](10))
    
    # call 855
    target.M855(clr.Reference[object]()); AreEqual(Flag.Value, 855)
    AssertError(TypeError, target.M855, clr.Reference[int](10))
    
    # call 854 and 855 with Reference[bool]
    target.M854(clr.Reference[bool](True)); AreEqual(Flag.Value, 854)
    target.M855(clr.Reference[bool](True)); AreEqual(Flag.Value, 855)
    
    # practical
    ref = clr.Reference[int]()
    ref2 = clr.Reference[int]()
    ref.Value = 300
    ref2.Value = 100
    ## M860(ref arg1, arg2, out arg3): arg3 = arg1 + arg2; arg1 = 100;
    x = target.M860(ref, 200, ref2)
    AreEqual(x, None)
    AreEqual(ref.Value, 100)
    AreEqual(ref2.Value, 500)
    
    # pass one clr.Reference(), and leave the other one open
    ref.Value = 300
    AssertError(TypeError, target.M860, ref, 200)
    
    # the other way
    x = target.M860(300, 200)
    AreEqual(x, (100, 500))
    
    # GOtherConcern<T>            
    target = GOtherConcern[int]()
    for x in [100, 200L, 4.56, myint1]:
        target.M100(x)
        AreEqual(Flag.Value, 100); Flag.Value = 99
    
    GOtherConcern[int].M100(target, 200)
    AreEqual(Flag.Value, 100); Flag.Value = 99
    AssertError(TypeError, target.M100, 'abc')
    AssertError(OverflowError, target.M100, 12345678901234)
    
def test_iterator_sequence():
    class C: 
        def __init__(self):  self.x = 0
        def __iter__(self):  return self
        def next(self): 
            if self.x < 10: 
                y = self.x
                self.x += 1
                return y
            else: 
                self.x = 0
                raise StopIteration
        def __len__(self): return 10
        
    # different size
    c = C()
    list1 = [1, 2, 3]
    tuple1 = [4, 5, 6, 7]
    str1 = "890123"    
    all = (list1, tuple1, str1, c)
    
    target = COtherConcern()
    
    for x in all:
        # IEnumerable / IEnumerator
        target.M620(x)
        AreEqual(Flag.Value, len(x)); Flag.Value = 0
        AssertError(TypeError, target.M621, x)

        # IEnumerable<char> / IEnumerator<char>
        target.M630(x)
        AreEqual(Flag.Value, len(x)); Flag.Value = 0
        AssertError(TypeError, target.M631, x)

        # IEnumerable<int> / IEnumerator<int>
        target.M640(x)
        AreEqual(Flag.Value, len(x)); Flag.Value = 0
        AssertError(TypeError, target.M641, x)

    # IList / IList<char> / IList<int>
    for x in (list1, tuple1):
        target.M622(x)
        AreEqual(Flag.Value, len(x))                

        target.M632(x)
        AreEqual(Flag.Value, len(x))                

        target.M642(x)
        AreEqual(Flag.Value, len(x))

    for x in (str1, c):
        AssertError(TypeError, target.M622, x)
        AssertError(TypeError, target.M632, x)
        AssertError(TypeError, target.M642, x)
       
def test_explicit_inheritance():
    target = CInheritMany1()
    Assert(not hasattr(target, "M"))
    try: target.M()
    except AttributeError: pass
    else: Fail("Expected AttributeError, got none")
    I1.M(target); AreEqual(Flag.Value, 100); Flag.Value = 0
    
    target = CInheritMany2()
    target.M(); AreEqual(Flag.Value, 201)
    I1.M(target); AreEqual(Flag.Value, 200)
    
    target = CInheritMany3()
    Assert(not hasattr(target, "M"))
    try: target.M()
    except AttributeError: pass
    else: Fail("Expected AttributeError, got none")
    I1.M(target); AreEqual(Flag.Value, 300)
    I2.M(target); AreEqual(Flag.Value, 301)
    
    target = CInheritMany4()
    target.M(); AreEqual(Flag.Value, 401)
    I3[object].M(target); AreEqual(Flag.Value, 400)
    AssertError(TypeError, I3[int].M, target)
    
    target = CInheritMany5()
    I1.M(target); AreEqual(Flag.Value, 500)
    I2.M(target); AreEqual(Flag.Value, 501)
    I3[object].M(target); AreEqual(Flag.Value, 502)
    target.M(); AreEqual(Flag.Value, 503)
    
    target = CInheritMany6[int]()
    target.M(); AreEqual(Flag.Value, 601)
    I3[int].M(target); AreEqual(Flag.Value, 600)
    AssertError(TypeError, I3[object].M, target)

    target = CInheritMany7[int]()
    Assert(not hasattr(target, "M"))
    try: target.M()
    except AttributeError: pass
    else: Fail("Expected AttributeError, got none")
    I3[int].M(target); AreEqual(Flag.Value, 700)
    
    target = CInheritMany8()
    Assert(not hasattr(target, "M"))
    try: target.M()
    except AttributeError: pass
    else: Fail("Expected AttributeError, got none")
    I1.M(target); AreEqual(Flag.Value, 800); Flag.Value = 0
    I4.M(target, 100); AreEqual(Flag.Value, 801)
    # target.M(100) ????
    
    # original repro
    from System.Collections.Generic import Dictionary
    d = Dictionary[object,object]()
    d.GetEnumerator() # not throw
        
print '>>>> methods in reference type'
target = CNoOverloads()
run_test(__name__)

