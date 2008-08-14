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

# COM Interop tests for IronPython
from lib.assert_util import skiptest
skiptest("win32", "silverlight")
from lib.cominterop_util import *
from System import *
from System.Runtime.InteropServices import COMException, DispatchWrapper
from clr import StrongBox

com_type_name = "DlrComLibrary.InOutParams"

#------------------------------------------------------------------------------
# Create a COM object
com_obj = getRCWFromProgID(com_type_name)

#------------------------------------------------------------------------------

simple_primitives_data = [
     ("mByte", Byte,  2, 4),
     ("mBstr", str, "a", "aa"),
     ("mDouble", float, 2.5, 4.5)
     ]
     
#Test calling simple types to validate marshalling.
def test_types():
    for funcName, type, inp, output in simple_primitives_data:        
        func = getattr(com_obj, funcName)
        strongBoxVar = StrongBox[type](inp)
        func(strongBoxVar)
        AreEqual(strongBoxVar.Value, output)
        
    strongBoxIDisp = StrongBox[object](com_obj)    
    com_obj.mIDispatch(strongBoxIDisp);
    AreEqual(strongBoxIDisp.Value, com_obj)
    
#Test different signatures of the functions
def test_calling_signatures():
    f1 = 2.5;
    f2 = 5.5;
    a = StrongBox[float](f1)
    b = StrongBox[float](f2)
    com_obj.mTwoInOutParams(a,b) #The function adds two to every parameter
    AreEqual(a.Value, f1+2) 
    AreEqual(b.Value, f1 + f2+2)
    
    if not preferComDispatch:    
        a = StrongBox[System.Decimal](3)    
        com_obj.mInAndInOutParams(2,a)
        AreEqual(a.Value, 2)
    
    now = System.DateTime.Now
    a = StrongBox[System.DateTime](now)
    b = StrongBox[System.DateTime](now)
    com_obj.mOutAndInOutParams(a,b)
    
#Try calling the COM function with params passed in as if they were in params. 
def test_as_inparams():
    for funcName, type, inp, output in simple_primitives_data:
        func = getattr(com_obj, funcName)
        func(inp)

    com_obj.mIDispatch(com_obj)
    com_obj.mTwoInOutParams(2,4)
	
#Validate ref params - they should have the same behaviour as in/out params.
def test_ref_params():
    f1 = 3.5;
    a = StrongBox[float](f1)
    com_obj.mSingleRefParam(a)
    AreEqual(a.Value, f1+ 2)
    
    com_obj.mSingleRefParam(5.2)    
    	
    str1 = "a"
    a = StrongBox[str](str1)
    b = StrongBox[object](com_obj)
    com_obj.mTwoRefParams(a,b)
    AreEqual(a.Value, "a")
    AreEqual(b.Value, com_obj)
    
#Try passing null to methods which accept pointers. TypeError should be thrown for all except strings
def test_passing_null():
	#Merlin Bug 323996
    if not preferComDispatch:
        AreEqual(com_obj.mBstr(None), "a")
        AssertError(TypeError, com_obj.mByte, None)
        AssertError(TypeError, com_obj.mSingleRefParam, None) 
    
        b = StrongBox[object](None)
        AssertError(TypeError, com_obj.mTwoRefParams, "a",b)
        
        a = StrongBox[object](None)
        AssertError(TypeError, com_obj.mTwoInOutParams, None, 3)

#------------------------------------------------------------------------------
run_com_test(__name__, __file__)