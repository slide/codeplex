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

from lib.assert_util import skiptest
skiptest("silverlight")
from lib.cominterop_util import *

###############################################################################
##GLOBALS######################################################################

com_type_name = "DlrComLibrary.DispEvents"
com_obj = getRCWFromProgID(com_type_name)
N = 3 #number of events to send
HANDLER_CALL_COUNT = 0  #number of events received

###############################################################################
##HELPERS######################################################################

def handler_helper(e_trigger, com_event, expected_retval, event_handlers):
    global HANDLER_CALL_COUNT
    
    for handler in event_handlers:
        
        #send an event with no handlers to ensure the test is OK
        HANDLER_CALL_COUNT = 0
        e_trigger()
        AreEqual(HANDLER_CALL_COUNT, 0)
        
        #add the handler and verify events are received
        try:
            com_event += handler
            for i in xrange(N):
                retval = e_trigger()
                AreEqual(retval, expected_retval)
                AreEqual(HANDLER_CALL_COUNT, i+1)
        
        except Exception, e:
                    print "handler_helper(", e_trigger, com_event, expected_retval, handler, ")"
                    raise e
        finally:
            #remove the handler
            com_event -= handler
    
        #send an event with no handlers to ensure removal
        HANDLER_CALL_COUNT = 0
        e_trigger()
        AreEqual(HANDLER_CALL_COUNT, 0)


def bad_handler_signature_helper(e_trigger, com_event, bad_arg_handlers):
    global HANDLER_CALL_COUNT
    for handler in bad_arg_handlers:
        
        #send an event with no handlers to ensure the test is OK
        HANDLER_CALL_COUNT = 0
        e_trigger()
        AreEqual(HANDLER_CALL_COUNT, 0)
        
        #add the handler and verify events are received
        com_event += handler

        try:
            e_trigger()
            Fail("Trying to call " + str(handler) + " as an event handler should have failed")
        except Exception, e:
            pass

        AreEqual(HANDLER_CALL_COUNT, 0)
            
        #send an event with no handlers to ensure removal
        com_event -= handler
        HANDLER_CALL_COUNT = 0
        e_trigger()
        AreEqual(HANDLER_CALL_COUNT, 0)        


###############################################################################
##SANITY CHECKS################################################################
#--One unique handler
ONE_HANDLER_COUNT = 0
ONE_HANDLER_VAL = None

@skip("multiple_execute")
def test_one_handler():
    
    def one_handler(a):
        global ONE_HANDLER_COUNT
        global ONE_HANDLER_VAL
    
        ONE_HANDLER_COUNT += 1
        ONE_HANDLER_VAL = a
    
    expected_count = 0
    expected_val = None
    
    if not preferComDispatch: #Merlin 384358
        com_event = com_obj.eInUshort
    else:
        com_event = com_obj.Event_eInUshort
    
    #Preliminary checks
    AreEqual(expected_count, ONE_HANDLER_COUNT)
    AreEqual(expected_val, None)
    
    #Send N events w/ no handlers attached
    for i in xrange(N):
        com_obj.triggerUShort(i)
        AreEqual(expected_count, ONE_HANDLER_COUNT)
        AreEqual(expected_val, ONE_HANDLER_VAL)
        
    #Attach one event handler
    com_event += one_handler
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 1
        expected_val = i
        AreEqual(expected_count, ONE_HANDLER_COUNT)
        AreEqual(expected_val, ONE_HANDLER_VAL)
    
    #Remove it
    com_event -= one_handler
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        AreEqual(expected_count, ONE_HANDLER_COUNT)
        AreEqual(expected_val, ONE_HANDLER_VAL)
    
    #Re-add the event handler
    com_event += one_handler
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 1
        expected_val = i
        AreEqual(expected_count, ONE_HANDLER_COUNT)
        AreEqual(expected_val, ONE_HANDLER_VAL)
    
    #Remove it
    com_event -= one_handler
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        AreEqual(expected_count, ONE_HANDLER_COUNT)
        AreEqual(expected_val, ONE_HANDLER_VAL)


#--Two identical handlers
TWO_IDENT_HANDLERS_COUNT = 0
TWO_IDENT_HANDLERS_VAL = None

@skip("multiple_execute")
def test_two_ident_handlers():
    
    def two_ident_handlers(a):
        global TWO_IDENT_HANDLERS_COUNT
        global TWO_IDENT_HANDLERS_VAL
    
        TWO_IDENT_HANDLERS_COUNT += 1
        TWO_IDENT_HANDLERS_VAL = a
    
    
    expected_count = 0
    expected_val = None
    
    if not preferComDispatch: #Merlin 384358
        com_event = com_obj.eInUshort
    else:
        com_event = com_obj.Event_eInUshort
    
    #Preliminary checks
    AreEqual(expected_count, TWO_IDENT_HANDLERS_COUNT)
    AreEqual(expected_val, None)
    
    #Send N events w/ no handlers attached
    for i in xrange(N):
        com_obj.triggerUShort(i)
        AreEqual(expected_count, TWO_IDENT_HANDLERS_COUNT)
        AreEqual(expected_val, TWO_IDENT_HANDLERS_VAL)
       
         
    #Attach two event handlers
    for i in xrange(2): 
        com_event += two_ident_handlers
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 2
        expected_val = i
        AreEqual(expected_count, TWO_IDENT_HANDLERS_COUNT)
        AreEqual(expected_val, TWO_IDENT_HANDLERS_VAL)
    
    #Remove one event handler
    com_event -= two_ident_handlers
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 1
        expected_val = i
        AreEqual(expected_count, TWO_IDENT_HANDLERS_COUNT)
        AreEqual(expected_val, TWO_IDENT_HANDLERS_VAL)
    
    #Add one event handler
    com_event += two_ident_handlers
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 2
        expected_val = i
        AreEqual(expected_count, TWO_IDENT_HANDLERS_COUNT)
        AreEqual(expected_val, TWO_IDENT_HANDLERS_VAL)
    
    #Remove them both
    for i in xrange(2):
        com_event -= two_ident_handlers
    
    #Send N events w/ no handlers attached
    for i in xrange(N):
        com_obj.triggerUShort(i)
        AreEqual(expected_count, TWO_IDENT_HANDLERS_COUNT)
        AreEqual(expected_val, TWO_IDENT_HANDLERS_VAL)
    
    #Re-add the event handlers
    for i in xrange(2):
        com_event += two_ident_handlers
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 2
        expected_val = i
        AreEqual(expected_count, TWO_IDENT_HANDLERS_COUNT)
        AreEqual(expected_val, TWO_IDENT_HANDLERS_VAL)
    
    #Remove them
    for i in xrange(2):
        com_event -= two_ident_handlers
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        AreEqual(expected_count, TWO_IDENT_HANDLERS_COUNT)
        AreEqual(expected_val, TWO_IDENT_HANDLERS_VAL)


#--Two unique handlers
TWO_UNIQUE_HANDLERS_COUNT = 0
TWO_UNIQUE_HANDLERS_VAL_A = None
TWO_UNIQUE_HANDLERS_VAL_B = None

@skip("multiple_execute")
def test_two_unique_handlers():
    def two_unique_handlers_a(a):
        global TWO_UNIQUE_HANDLERS_COUNT
        global TWO_UNIQUE_HANDLERS_VAL_A
    
        TWO_UNIQUE_HANDLERS_COUNT += 1
        TWO_UNIQUE_HANDLERS_VAL_A = a
    
    def two_unique_handlers_b(a):
        global TWO_UNIQUE_HANDLERS_COUNT
        global TWO_UNIQUE_HANDLERS_VAL_B
    
        TWO_UNIQUE_HANDLERS_COUNT += 1
        TWO_UNIQUE_HANDLERS_VAL_B = a
    
    expected_count = 0
    expected_val_a = None
    expected_val_b = None
    
    if not preferComDispatch: #Merlin 384358
        com_event = com_obj.eInUshort
    else:
        com_event = com_obj.Event_eInUshort
    
    #Preliminary checks
    AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
    AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
    AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
    
    #Send N events w/ no handlers attached
    for i in xrange(N):
        com_obj.triggerUShort(i)
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
    
    #Attach handler A
    com_event += two_unique_handlers_a
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 1
        expected_val_a = i
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
    
    #Attach handler B
    com_event += two_unique_handlers_b
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 2
        expected_val_a = i
        expected_val_b = i
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
     
    #Remove handler A
    com_event -= two_unique_handlers_a
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 1
        expected_val_b = i
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
    
    #Attach handler A
    com_event += two_unique_handlers_a
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 2
        expected_val_a = i
        expected_val_b = i
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
    
    #Remove handler A
    com_event -= two_unique_handlers_a
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 1
        expected_val_b = i
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
    
    #Remove handler B
    com_event -= two_unique_handlers_b
    
    #Send N events w/ no handlers attached
    for i in xrange(N):
        com_obj.triggerUShort(i)
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
        
        
    #Attach handlers A and B
    com_event += two_unique_handlers_a
    com_event += two_unique_handlers_b
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 2
        expected_val_a = i
        expected_val_b = i
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
    
    #Attach second B handler
    com_event += two_unique_handlers_b
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 3
        expected_val_a = i
        expected_val_b = i
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
        
    #Remove second B handler
    com_event -= two_unique_handlers_b
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        expected_count += 2
        expected_val_a = i
        expected_val_b = i
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
    
    #Remove A and B handlers
    com_event -= two_unique_handlers_a
    com_event -= two_unique_handlers_b
    
    #Send N events
    for i in xrange(N):
        com_obj.triggerUShort(i)
        AreEqual(expected_count, TWO_UNIQUE_HANDLERS_COUNT)
        AreEqual(expected_val_a, TWO_UNIQUE_HANDLERS_VAL_A)
        AreEqual(expected_val_b, TWO_UNIQUE_HANDLERS_VAL_B)
        
        
###############################################################################
##EVENT HANDLER SIGNATURES#####################################################


#--POSITIVE--------------------------------------------------------------------
def test_eNull():
    global HANDLER_CALL_COUNT
    e_trigger = com_obj.triggerNull
    if not preferComDispatch:
        com_event = com_obj.eNull
    else:
        com_event = com_obj.Event_eNull
    
    #--typical handler implementations
    def f1():
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
    
    def f2(*args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too many args:" + str(args))
    
    def f3(**kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
    
    def f4(*args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too many args:" + str(args))
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
    
    event_handlers = [f1, f2, f3, f4]    
    handler_helper(e_trigger, com_event, None, event_handlers)
 

def test_eInOutretBool():
    if not preferComDispatch:
        #Merlin 384378
        return
    
    global HANDLER_CALL_COUNT
    e_trigger = com_obj.triggerInOutretBool
    if not preferComDispatch:
        com_event = com_obj.eInOutretBool
    else:
        com_event = com_obj.Event_eInOutretBool
    
    #--typical handler implementations
    def f1(a):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        return a
    
    def f2(*args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=1: raise TypeError("Too few/many args:" + str(args))
        return args[0]
        
    def f3(a, *args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too many args:" + str(args))
        return a
    
    def f4(a, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return a
    
    def f5(*args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=1: raise TypeError("Too few/many args:" + str(args))
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return args[0]
        
    def f6(a, *args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too few/many args:" + str(args))
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return a
    
    event_handlers = [f1, f2, f3, f4, f5, f6]
    
    handler_helper(lambda: e_trigger(True), com_event,  
                    False, #Merlin 386344
                    event_handlers)
    handler_helper(lambda: e_trigger(False), com_event, False, event_handlers)   
    
    
def test_eInOutBstr():
    if not preferComDispatch:
        #Merlin 384378
        return
    
    global HANDLER_CALL_COUNT
    e_trigger = com_obj.triggerInOutBstr
    if not preferComDispatch:
        com_event = com_obj.eInOutBstr
    else:
        com_event = com_obj.Event_eInOutBstr
    
    #--typical handler implementations
    def f1(a):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        return a
    
    def f2(*args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=1: raise TypeError("Too few/many args:" + str(args))
        return args[0]
        
    def f3(a, *args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too many args:" + str(args))
        return a
    
    def f4(a, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return a
    
    def f5(*args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=1: raise TypeError("Too few/many args:" + str(args))
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return args[0]
        
    def f6(a, *args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too few/many args:" + str(args))
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return a
    
    event_handlers = [f1, f2, f3, f4, f5, f6]
    
    try:
        handler_helper(lambda: e_trigger(""), com_event,  "", event_handlers)
        handler_helper(lambda: e_trigger("abc"), com_event, "abc", event_handlers)
    except EnvironmentError, e:
        print "Merlin 386367"

def test_eInUshort():
    global HANDLER_CALL_COUNT
    e_trigger = com_obj.triggerUShort
    if not preferComDispatch:
        com_event = com_obj.eInUshort
    else:
        com_event = com_obj.Event_eInUshort
    
    def f1(a):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        return
    
    def f2(*args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=1: raise TypeError("Too few/many args:" + str(args))
        return
        
    def f3(a, *args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too many args:" + str(args))
        return
    
    def f4(a, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return
    
    def f5(*args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=1: raise TypeError("Too few/many args:" + str(args))
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return
        
    def f6(a, *args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too few/many args:" + str(args))
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return
    
    event_handlers = [f1, f2, f3, f4, f5, f6]
    handler_helper(lambda: e_trigger(0), com_event, None, event_handlers)
    handler_helper(lambda: e_trigger(42), com_event, None, event_handlers)        

def test_eNullShort():
    global HANDLER_CALL_COUNT
    e_trigger = com_obj.triggerNullShort
    if not preferComDispatch:
        com_event = com_obj.eNullShort
    else:
        com_event = com_obj.Event_eNullShort
    
    #--typical handler implementations
    def f1():
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        return 42
    
    def f2(*args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too many args:" + str(args))
        return 42
    
    def f3(**kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return 42
    
    def f4(*args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        if len(args)!=0: raise TypeError("Too many args:" + str(args))
        if len(kwargs.keys())!=0: raise TypeError("Too many kwargs:" + str(kwargs))
        return 42
    
    event_handlers = [f1, f2, f3, f4]    
    handler_helper(e_trigger, com_event, None, event_handlers)
    
    
#--NEGATIVE HANDLER SIGNATURES-------------------------------------------------    
def test_eNull_neg_handler_signatures():
    global HANDLER_CALL_COUNT
    e_trigger = com_obj.triggerNull
    if not preferComDispatch:
        com_event = com_obj.eNull
    else:
        com_event = com_obj.Event_eNull
    
    #--bad handler implementations
    def f1(a):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
    
    def f2(a, b):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f3(a, *args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f4(a, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f5(a, *args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    

    bad_arg_handlers = [f1, f2, 
                        #f3, #Merlin 384332
                        #f4, #Merlin 384332
                        #f5, #Merlin 384332
                        ]
    #Merlin 384336
    if preferComDispatch:
        bad_arg_handlers = []                        
    
    bad_handler_signature_helper(e_trigger, com_event, bad_arg_handlers)


def test_eInOutretBool_neg_handler_signatures():
    e_trigger = com_obj.triggerInOutretBool
    if not preferComDispatch:
        com_event = com_obj.eInOutretBool
    else:
        com_event = com_obj.Event_eInOutretBool
        
        
    #--bad handler implementations
    def f1():
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
    
    def f2(a, b):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f3(a, b, *args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f4(a, b, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f5(a, b, *args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    bad_arg_handlers = [f1, 
                        #f2, #Merlin 384332
                        #f3, #Merlin 384332 
                        #f4, #Merlin 384332
                        #f5, #Merlin 384332
                        ]
    #Merlin 384336
    if preferComDispatch:
        bad_arg_handlers = []                        

    bad_handler_signature_helper(lambda: e_trigger(True), com_event, bad_arg_handlers)
        
  
def test_eInOutBstr_neg_handler_signatures():
    e_trigger = com_obj.triggerInOutBstr
    if not preferComDispatch:
        com_event = com_obj.eInOutBstr
    else:
        com_event = com_obj.Event_eInOutBstr
        
    #--bad handler implementations
    def f1():
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
    
    def f2(a, b):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f3(a, b, *args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f4(a, b, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f5(a, b, *args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    bad_arg_handlers = [f1, 
                        #f2, #Merlin 384332
                        #f3, #Merlin 384332 
                        #f4, #Merlin 384332
                        #f5, #Merlin 384332
                        ]
    #Merlin 384336
    if preferComDispatch:
        bad_arg_handlers = []                        

    bad_handler_signature_helper(lambda: e_trigger("abc"), com_event, bad_arg_handlers)
    


def test_eInUshort_neg_handler_signatures():
    e_trigger = com_obj.triggerUShort
    if not preferComDispatch:
        com_event = com_obj.eInUshort
    else:
        com_event = com_obj.Event_eInUshort
        
        
    #--bad handler implementations
    def f1():
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
    
    def f2(a, b):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f3(a, b, *args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f4(a, b, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    def f5(a, b, *args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        
    bad_arg_handlers = [f1, 
                        f2, 
                        #f3, #Merlin 384332 
                        #f4, #Merlin 384332
                        #f5, #Merlin 384332
                        ]
    #Merlin 384336
    if preferComDispatch:
        bad_arg_handlers = []                        

    bad_handler_signature_helper(lambda: e_trigger(1), com_event, bad_arg_handlers)


def test_eNullShort_neg_handler_signatures():
    global HANDLER_CALL_COUNT
    e_trigger = com_obj.triggerNullShort
    if not preferComDispatch:
        com_event = com_obj.eNullShort
    else:
        com_event = com_obj.Event_eNullShort
    
    #--bad handler implementations
    def f1(a):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        return 42
    
    def f2(a, b):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        return 42
        
    def f3(a, *args):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        return 42
        
    def f4(a, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        return 42
        
    def f5(a, *args, **kwargs):
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        raise Exception("Bad number of args should trigger DispException")
        return 42
    

    bad_arg_handlers = [f1, f2, 
                        #f3, #Merlin 384332
                        #f4, #Merlin 384332
                        #f5, #Merlin 384332
                        ]
    #Merlin 384336
    if preferComDispatch:
        bad_arg_handlers = []                        
    
    bad_handler_signature_helper(e_trigger, com_event, bad_arg_handlers)
    
#--NEGATIVE HANDLER RETURN VALUES----------------------------------------------
def test_eNull_neg_handler_return_values():
    global HANDLER_CALL_COUNT
    HANDLER_CALL_COUNT = 0
    expected_call_count = 0
    e_trigger = com_obj.triggerNull
    
    if not preferComDispatch:
        com_event = com_obj.eNull
    else:
        com_event = com_obj.Event_eNull

    for retVal in [1, "", "abc", False, []]:
        def bad_handler():
            global HANDLER_CALL_COUNT
            HANDLER_CALL_COUNT+=1
            return retVal

        com_event += bad_handler
        
        #Expect this to throw an exception
        #AssertError(EnvironmentError, e_trigger)
        e_trigger()  #Merlin 384367
        
        #Also expect the HANDLER_CALL_COUNT to have been incremented
        expected_call_count += 1
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)
    
        #remove handler and send an event to ensure removal
        com_event -= bad_handler
        e_trigger()
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)
        

def test_eInOutretBool_neg_handler_return_values():
    global HANDLER_CALL_COUNT
    HANDLER_CALL_COUNT = 0
    expected_call_count = 0
    e_trigger = com_obj.triggerInOutretBool
    
    if not preferComDispatch:
        com_event = com_obj.eInOutretBool
    else:
        com_event = com_obj.Event_eInOutretBool
            
    for retVal in [3.14, "", "abc", []]:
        
        def bad_handler(a):
            global HANDLER_CALL_COUNT
            HANDLER_CALL_COUNT+=1
            return retVal

        if not preferComDispatch:
            continue #Merlin 384378 - TypeError
        com_event += bad_handler
        
        #Expect this to throw an exception
        #AssertError(EnvironmentError, e_trigger, True)
        e_trigger(True)  #Merlin 384367
        
        #Also expect the HANDLER_CALL_COUNT to have been incremented
        expected_call_count += 1
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)
    
        #remove handler and send an event to ensure removal
        com_event -= bad_handler
        e_trigger(True)
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)
    
    
def test_eInOutBstr_neg_handler_return_values():
    global HANDLER_CALL_COUNT
    HANDLER_CALL_COUNT = 0
    expected_call_count = 0
    e_trigger = com_obj.triggerInOutBstr
    
    if not preferComDispatch:
        com_event = com_obj.eInOutBstr
    else:
        com_event = com_obj.Event_eInOutBstr
            
    for retVal in [3.14, True, 42L, []]:
        
        def bad_handler(a):
            global HANDLER_CALL_COUNT
            HANDLER_CALL_COUNT+=1
            return retVal

        if not preferComDispatch:
            continue #Merlin 384378 - TypeError
        com_event += bad_handler
        
        #Expect this to throw an exception
        #AssertError(EnvironmentError, e_trigger, "abc")
        if preferComDispatch:
            print "Merlin 386367"  #cannot continue as bad_handler never gets called
            com_event -= bad_handler
            continue
        
        e_trigger("abc")  #Merlin 384367
        
        #Also expect the HANDLER_CALL_COUNT to have been incremented
        expected_call_count += 1
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)
    
        #remove handler and send an event to ensure removal
        com_event -= bad_handler
        e_trigger("abc")
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)


def test_eInUshort_neg_handler_return_values():
    global HANDLER_CALL_COUNT
    HANDLER_CALL_COUNT = 0
    expected_call_count = 0
    e_trigger = com_obj.triggerUShort
    
    if not preferComDispatch:
        com_event = com_obj.eInUshort
    else:
        com_event = com_obj.Event_eInUshort

    for retVal in [3.14, "", "abc", False, []]:
        def bad_handler(a):
            global HANDLER_CALL_COUNT
            HANDLER_CALL_COUNT+=1
            return retVal

        com_event += bad_handler
        
        #Expect this to throw an exception
        #AssertError(EnvironmentError, e_trigger, 42)
        e_trigger(42)  #Merlin 384367
        
        #Also expect the HANDLER_CALL_COUNT to have been incremented
        expected_call_count += 1
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)
    
        #remove handler and send an event to ensure removal
        com_event -= bad_handler
        e_trigger(42)
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)
    
    
def test_eNullShort_neg_handler_return_values():
    global HANDLER_CALL_COUNT
    HANDLER_CALL_COUNT = 0
    expected_call_count = 0
    e_trigger = com_obj.triggerNullShort
    
    if not preferComDispatch:
        com_event = com_obj.eNullShort
    else:
        com_event = com_obj.Event_eNullShort

    for retVal in [None, "", "abc", []]:
        def bad_handler():
            global HANDLER_CALL_COUNT
            HANDLER_CALL_COUNT+=1
            return retVal

        com_event += bad_handler
        
        #Expect this to throw an exception
        if not preferComDispatch:
            AssertError(EnvironmentError, e_trigger)  
        else:
            e_trigger()  #Merlin 384367
        
        #Also expect the HANDLER_CALL_COUNT to have been incremented
        expected_call_count += 1
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)
    
        #remove handler and send an event to ensure removal
        com_event -= bad_handler
        e_trigger()
        AreEqual(HANDLER_CALL_COUNT, expected_call_count)
    
    
###############################################################################
##MISC#########################################################################

def test_slow_handler_sta():
    from time import sleep
    global HANDLER_CALL_COUNT
 
    e_trigger = com_obj.triggerNull
    
    if not preferComDispatch:
        com_event = com_obj.eNull
    else:
        com_event = com_obj.Event_eNull
    
    def slow_handler():
        global HANDLER_CALL_COUNT
        HANDLER_CALL_COUNT += 1
        print "slow_handler...sleeping"
        sleep(5)
        HANDLER_CALL_COUNT += 1
     
    #send an event with no handlers to ensure the test is OK
    HANDLER_CALL_COUNT = 0
    e_trigger()
    AreEqual(HANDLER_CALL_COUNT, 0)
        
    #add the handler and verify events are received
    com_event += slow_handler
    e_trigger()
    #UNCOMMENT THIS UNDER MTA
    #AreEqual(HANDLER_CALL_COUNT, 1)
    #sleep(10)
    AreEqual(HANDLER_CALL_COUNT, 2)
    
    #remove the handler
    com_event -= slow_handler
    
    #send an event with no handlers to ensure it's removed
    HANDLER_CALL_COUNT = 0
    e_trigger()
    sleep(10)
    AreEqual(HANDLER_CALL_COUNT, 0)
    

def test_handler_spawns_thread():
    from time import sleep
    global HANDLER_CALL_COUNT
 
    e_trigger = com_obj.triggerNull
    
    if not preferComDispatch:
        com_event = com_obj.eNull
    else:
        com_event = com_obj.Event_eNull
    
    def thread_handler():
        #Start a thread which increments the HANDLER_CALL_COUNT
        import thread
        
        def f():
            global HANDLER_CALL_COUNT
            from time import sleep
            HANDLER_CALL_COUNT += 1
            sleep(10)
            HANDLER_CALL_COUNT += 1
        
        thread.start_new_thread(f, ())
        
     
    #send an event with no handlers to ensure the test is OK
    HANDLER_CALL_COUNT = 0
    e_trigger()
    AreEqual(HANDLER_CALL_COUNT, 0)
        
    #add the handler and verify events are received
    com_event += thread_handler
    e_trigger()
    sleep(5) #Seems fragile
    AreEqual(HANDLER_CALL_COUNT, 1)
    sleep(15)
    AreEqual(HANDLER_CALL_COUNT, 2)
    
    #remove the handler
    com_event -= thread_handler
    
    #send an event with no handlers to ensure it's removed
    HANDLER_CALL_COUNT = 0
    e_trigger()
    sleep(10)
    AreEqual(HANDLER_CALL_COUNT, 0)

def test_handler_calls_caller():
    global HANDLER_CALL_COUNT
 
    e_trigger = com_obj.triggerNull
    
    if not preferComDispatch:
        com_event = com_obj.eNull
    else:
        com_event = com_obj.Event_eNull
    
    def call_caller_handler():
        global HANDLER_CALL_COUNT
        
        if HANDLER_CALL_COUNT==0:
            HANDLER_CALL_COUNT += 1
            e_trigger()
        else:
            HANDLER_CALL_COUNT += 1
        
    #send an event with no handlers to ensure the test is OK
    HANDLER_CALL_COUNT = 0
    e_trigger()
    AreEqual(HANDLER_CALL_COUNT, 0)
        
    #add the handler and verify events are received
    com_event += call_caller_handler
    e_trigger()
    AreEqual(HANDLER_CALL_COUNT, 2)
    
    #remove the handler
    com_event -= call_caller_handler
    
    #send an event with no handlers to ensure it's removed
    HANDLER_CALL_COUNT = 0
    e_trigger()
    AreEqual(HANDLER_CALL_COUNT, 0)

def test_handler_raises():
    global HANDLER_CALL_COUNT
 
    e_trigger = com_obj.triggerNull
    
    if not preferComDispatch:
        com_event = com_obj.eNull
    else:
        com_event = com_obj.Event_eNull
    
    def except_handler():
        global HANDLER_CALL_COUNT
        raise Exception("bad")
        HANDLER_CALL_COUNT += 1
        
    #send an event with no handlers to ensure the test is OK
    HANDLER_CALL_COUNT = 0
    e_trigger()
    AreEqual(HANDLER_CALL_COUNT, 0)
        
    #add the handler and verify events are received
    com_event += except_handler

    AssertError(EnvironmentError, e_trigger)
    AreEqual(HANDLER_CALL_COUNT, 0)
    
    #remove the handler
    com_event -= except_handler
    
    #send an event with no handlers to ensure it's removed
    HANDLER_CALL_COUNT = 0
    e_trigger()
    AreEqual(HANDLER_CALL_COUNT, 0)    

    
###############################################################################
##MAIN#########################################################################
run_com_test(__name__, __file__)