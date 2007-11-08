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

import nt
import sys
import rulediff
        
from common import *

def run_cpython(test):
    return launch(cpython_executable, test)

def run_ipython(test):
    return launch(ipython_executable, test)

def get_sbs_tests():
    not_run_tests = [ ]
    if sys.platform == "cli":
        import System
        if System.IntPtr.Size == 8:
            not_run_tests = ["sbs_exceptions.py"]	

    import nt
    return [x[:-3] for x in nt.listdir(compat_test_path) 
                   if x.startswith("sbs_") and x.endswith(".py") and (x.lower() not in not_run_tests)]

success = failure = compfail = 0
   
def run_sbs_test(l):
    global success, failure
    
    exceptions = []
    for test in l:
        try:
            print ">>>>", test
            __import__(test)
            success += 1
        except Exception, e:
            print "*FAIL*"
            failure += 1
            exceptions.append((test, e))
            
    print "----------------------------------------"
    if failure > 0 or len(exceptions) > 0:
        print " Run & Compare:   !!! FAILED !!!"
        for exception in exceptions:
            print '------------------------------------'
            print 'Test %s failed' % exception[0]
            print exception[1]
            if sys.platform == "cli":
                import System
                if '-X:ExceptionDetail' in System.Environment.GetCommandLineArgs():
                    print 'CLR Exception: ',
                    print exception[1].clsException

    else:
        print " Run & Compare:   !!! SUCCESS !!!"
    print "----------------------------------------"
    print " Tests ran: " + str(success + failure), " Success: " + str(success)  + " Failure:   " + str(failure)
    print "----------------------------------------"
    
    if failure:
        raise AssertionError, "Failed"

def run(type="long", tests = "full", compare=True):
    if type in ["short", "medium"]:
        return 1

    ensure_future_present()
    
    print "*** generated result logs/scripts @", compat_test_path
    if tests == "full": tests = get_sbs_tests()
    
    run_sbs_test(tests)

if __name__ == "__main__":
    args = sys.argv
    
    bCompare = sys.platform == "cli"
    if len(args) == 1 :
        run(compare = bCompare)
    else:
        run(tests = [ x[:-3] for x in args[1:] ], compare = bCompare)
        
