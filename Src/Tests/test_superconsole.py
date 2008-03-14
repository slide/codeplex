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

from   time import sleep
import sys
import re

from   System.Diagnostics import Process, ProcessWindowStyle
from   System.IO import File
import clr

#------------------------------------------------------------------------------
#--Globals

#if this is a debug build and the assemblies are being saved...peverify is run.
#for the test to pass, Maui assemblies must be in the AssembliesDir
if is_peverify_run:
    clr.AddReference("Microsoft.Scripting")
    from Microsoft.Scripting.Runtime import ScriptDomainManager
    from System.IO import Path

    tempMauiDir = Path.GetTempPath()    
    
    print "Copying Maui.Core.dll to %s for peverify..." % (tempMauiDir)
    if not File.Exists(tempMauiDir + '\\Maui.Core.dll'):
        File.Copy(testpath.rowan_root + '\\Languages\\IronPython\\External\\Maui\\Maui.Core.dll',
                  tempMauiDir + '\\Maui.Core.dll')    

#Cleanup the last run
for t_name in ['ip_session.log', 'ip_session_stderr.log']:
    if File.Exists(t_name):
        File.Delete(t_name)

#------------------------------------------------------------------------------
#--Helper functions
def getTestOutput():
    '''
    Returns stdout and stderr output for a console test.
    '''
    
    #On some platforms 'ip_session.log' is not immediately created after
    #calling the 'close' method of the file object writing to 'ip_session.log'.
    #Give it a few seconds to catch up.
    for i in xrange(5):
        if "ip_session.log" in nt.listdir(nt.getcwd()):
            tfile = open('ip_session.log', 'r')
            break
        from time import sleep
        print "Waiting for ip_session.log to be created..."
        sleep(1)
    
    outlines = tfile.readlines()
    tfile.close()
    
    errlines = []
    if File.Exists('ip_session_stderr.log'):
        tfile = open('ip_session_stderr.log', 'r')
        errlines = tfile.readlines()
        tfile.close()
        
    return (outlines, errlines)

def removePrompts(lines):
    return [line for line in lines if not line.startswith(">>>") and not line.startswith("...")]

def verifyResults(lines, testRegex):
    '''
    Verifies that a set of lines match a regular expression.
    '''
    lines = removePrompts(lines)
    chopped = ''.join([line[:-1] for line in lines])
    Assert(re.match(testRegex, chopped),
           "Expected Regular Expression=" + testRegex + "\nActual Lines=" + chopped)


#------------------------------------------------------------------------------
#--Preliminary setup

sys.path.append(testpath.rowan_root + '\\Languages\\IronPython\\External\\Maui')
try:
    clr.AddReference('Maui.Core.dll')
except:
    print "test_superconsole.py failed: cannot load Maui.Core assembly"
    sys.exit(int(is_snap))

from Maui.Core import App
proc = Process()
proc.StartInfo.FileName = sys.executable
proc.StartInfo.WorkingDirectory = testpath.rowan_root + '\\Languages\\IronPython\\Tests'
proc.StartInfo.Arguments = '-X:TabCompletion -X:AutoIndent -X:ColorfulConsole'
proc.StartInfo.UseShellExecute = True	
proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal
proc.StartInfo.CreateNoWindow = False
started = proc.Start()

try:
    superConsole = App(proc.Id)
except:
    print "test_superconsole.py failed: cannot initialize App object (probably running as service, or in minimized remote window"
    proc.Kill()
    sys.exit(0)
    
superConsole.SendKeys('from pretest import *{ENTER}')


#------------------------------------------------------------------------------
#--Tests
def test_newlines():
    '''
    Ensure empty lines do not break the console.
    '''
    #test
    superConsole.SendKeys('outputRedirectStart{(}{)}{ENTER}')
    superConsole.SendKeys('{ENTER}')
    superConsole.SendKeys('None{ENTER}')
    superConsole.SendKeys('{ENTER}{ENTER}{ENTER}')
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    
    #verification
    for lines in getTestOutput():
        AreEqual(removePrompts(lines), [])

def test_string_exception():
    '''
    An exception thrown should appear in stderr.
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}{)}{ENTER}')
    

    superConsole.SendKeys('raise "Some string exception"{ENTER}')
    print "CodePlex Work Item 12403"
    expected = ["Traceback (most recent call last):",
                "  File ", #CodePlex Work Item 12403
                "Some string exception",
                "", #CodePlex Work Item 12401
                ]

    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    #stdout should be empty
    AreEqual(removePrompts(getTestOutput()[0]), 
             [])
    #stderr should contain the exception             
    errlines = getTestOutput()[1]       
    for i in xrange(len(errlines)):
        Assert(errlines[i].startswith(expected[i]), str(errlines) + " != " + str(expected))         
    

@disabled("CodePlex Work Item 10928")
def test_unique_prefix_completion():
    '''
    Ensure that an attribute with a prefix unique to the dictionary is 
    properly completed.
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}{)}{ENTER}')
    testRegex = ""

    superConsole.SendKeys('print z{TAB}{ENTER}')
    testRegex += 'zoltar'
    superConsole.SendKeys('print yo{TAB}{ENTER}')
    testRegex += 'yorick'

    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)
    AreEqual(removePrompts(getTestOutput()[1]), 
             [])  

@disabled("CodePlex Work Item 10928")
def test_nonunique_prefix_completion():
    '''
    Ensure that tabbing on a non-unique prefix cycles through the available
    options.
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}{)}{ENTER}')
    testRegex = ""
    
    superConsole.SendKeys('print y{TAB}{ENTER}')
    superConsole.SendKeys('print y{TAB}{TAB}{ENTER}')
    testRegex += '(yorickyak|yakyorick)'

    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)
    AreEqual(removePrompts(getTestOutput()[1]), 
             [])  

def test_member_completion():
    '''
    Ensure that tabbing after 'ident.' cycles through the available options.
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}True{)}{ENTER}')
    testRegex = ""

    # 3.1: identifier is valid, we can get dict
    superConsole.SendKeys('print c.{TAB}{ENTER}')

    # it is *either* __doc__ ('Cdoc') or __module__ ('pretest')
    testRegex += '(Cdoc|pretest)'

    # 3.2: identifier is not valid
    superConsole.SendKeys('try:{ENTER}')

    # autoindent
    superConsole.SendKeys('print f.{TAB}x{ENTER}')

    # backup from autoindent
    superConsole.SendKeys('{BACKSPACE}except:{ENTER}')
    superConsole.SendKeys('print "EXC"{ENTER}{ENTER}{ENTER}')
    testRegex += 'EXC'
    
    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)
    
def test_autoindent():
    '''
    Auto-indent
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}True{)}{ENTER}')
    testRegex = ""
    
    superConsole.SendKeys("def f{(}{)}:{ENTER}print 'f!'{ENTER}{ENTER}")
    superConsole.SendKeys('f{(}{)}{ENTER}')
    testRegex += 'f!'

    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)

def test_backspace_and_delete():
    '''
    Backspace and delete
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}True{)}{ENTER}')
    testRegex = ""
    
    superConsole.SendKeys("print 'IQ{BACKSPACE}P'{ENTER}")
    testRegex += "IP"

    superConsole.SendKeys("print 'FW'{LEFT}{LEFT}{DELETE}X{ENTER}")
    testRegex += "FX"

    # 5.3: backspace over auto-indentation
    #   a: all white space
    #   b: some non-whitespace characters

    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)

def test_cursor_keys():
    '''
    Cursor keys
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}True{)}{ENTER}')
    testRegex = ""
    
    superConsole.SendKeys("print 'up'{ENTER}")
    testRegex += 'up'
    superConsole.SendKeys("print 'down'{ENTER}")
    testRegex += 'down'
    superConsole.SendKeys("{UP}{UP}{ENTER}") 
    testRegex += 'up'
    superConsole.SendKeys("{DOWN}{ENTER}")
    testRegex += 'down'

    superConsole.SendKeys("print 'up'{ENTER}{UP}{ENTER}")
    testRegex += 'upup'
    superConsole.SendKeys("print 'awy{LEFT}{LEFT}{RIGHT}a{RIGHT}'{ENTER}")
    testRegex += 'away'
    superConsole.SendKeys("print 'bad'{ESC}print 'good'{ENTER}")
    testRegex += 'good'
    superConsole.SendKeys("rint 'hom'{HOME}p{END}{LEFT}e{ENTER}")
    testRegex += 'home'
    
    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)

def test_control_character_rendering():
    '''
    Control-character rendering
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}{)}{ENTER}')
    testRegex = ""

    # Ctrl-D
    superConsole.SendKeys('print "^(d)^(d){LEFT}{DELETE}"{ENTER}')
    testRegex += chr(4)

    # check that Ctrl-C breaks an infinite loop (the test is that subsequent things actually appear)
    superConsole.SendKeys('while True: pass{ENTER}{ENTER}')
    superConsole.SendKeys('^(c)')
    print "CodePlex Work Item 12401"
    errors = [
                "Traceback (most recent call last):", #CodePlex Work Item 12401
                "  File", #CodePlex Work Item 12401
                "  File", #CodePlex Work Item 12401
                "KeyboardInterrupt",
                "", #CodePlex Work Item 12401
            ]

    # check that Ctrl-C breaks an infinite loop (the test is that subsequent things actually appear)
    superConsole.SendKeys('def foo{(}{)}:{ENTER}try:{ENTER}while True:{ENTER}pass{ENTER}')
    superConsole.SendKeys('{BACKSPACE}{BACKSPACE}except KeyboardInterrupt:{ENTER}print "caught"{ENTER}{BACKSPACE}{ENTER}')
    superConsole.SendKeys('print "after"{ENTER}{BACKSPACE}{ENTER}foo{(}{)}{ENTER}')    
    sleep(2)
    superConsole.SendKeys('^(c)')
    testRegex += 'caughtafter'

    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)
    #stderr should contain the exceptions       
    errlines = getTestOutput()[1]
    Assert("KeyboardInterrupt: " + newline in errlines, 
           "KeyboardInterrupt not found in:" + str(errlines))  
    #for i in xrange(len(errlines)):
    #    Assert(errlines[i].startswith(errors[i]), str(errlines) + " != " + str(errors))

def test_tab_insertion():
    '''
    Tab insertion
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}True{)}{ENTER}')
    testRegex = ""
    
    superConsole.SendKeys('print "x{TAB}{TAB}y"{ENTER}')
    testRegex += 'x    y'

    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)
    
def test_noeffect_keys():
    '''
    Make sure that home, delete, backspace, etc. at start have no effect
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}True{)}{ENTER}')
    testRegex = ""	
    
    superConsole.SendKeys('{BACKSPACE}{DELETE}{HOME}{LEFT}print "start"{ENTER}')
    testRegex += 'start'
    
    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)

def test_tab_completion_caseinsensitive():
    '''
    Tab-completion is case-insensitive (wrt input)
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}True{)}{ENTER}')
    testRegex = ""	
    
    superConsole.SendKeys('import System{ENTER}')
    superConsole.SendKeys('print System.r{TAB}{ENTER}')
    testRegex += "<type 'Random'>"
    
    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)

def test_history():
    '''
    Command history
    '''
    #setup
    superConsole.SendKeys('outputRedirectStart{(}True{)}{ENTER}')
    testRegex = ""	
    
    superConsole.SendKeys('print "first"{ENTER}')
    testRegex += 'first'
    superConsole.SendKeys('print "second"{ENTER}')
    testRegex += 'second'
    superConsole.SendKeys('print "third"{ENTER}')
    testRegex += 'third'
    superConsole.SendKeys('print "fourth"{ENTER}')
    testRegex += 'fourth'
    superConsole.SendKeys('print "fifth"{ENTER}')
    testRegex += 'fifth'
    superConsole.SendKeys('{UP}{UP}{UP}{ENTER}')
    testRegex += 'third'
    superConsole.SendKeys('{UP}{ENTER}')
    testRegex += 'third'
    superConsole.SendKeys('{UP}{UP}{UP}{DOWN}{ENTER}')
    testRegex += 'second'
    superConsole.SendKeys('{UP}{ENTER}')
    testRegex += 'second'
    superConsole.SendKeys('{DOWN}{ENTER}')
    testRegex += 'third'
    superConsole.SendKeys('{DOWN}{ENTER}')
    testRegex += 'fourth'
    superConsole.SendKeys('{DOWN}{ENTER}')
    testRegex += 'fifth'
    superConsole.SendKeys('{UP}{UP}{ESC}print "sixth"{ENTER}')
    testRegex += 'sixth'
    superConsole.SendKeys('{UP}{ENTER}')
    testRegex += 'sixth'
    superConsole.SendKeys('{UP}{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}{ENTER}')
    testRegex += 'sixth'
    
    #verification
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    verifyResults(getTestOutput()[0], testRegex)

def test_raw_input():
    '''
    '''
    superConsole.SendKeys('outputRedirectStart{(}{)}{ENTER}')
    superConsole.SendKeys('x = raw_input{(}"foo"{)}{ENTER}')
    superConsole.SendKeys('{ENTER}')
    superConsole.SendKeys('print x{ENTER}')
    
    superConsole.SendKeys('x = raw_input{(}"foo"{)}{ENTER}')
    superConsole.SendKeys('abc{ENTER}')
    superConsole.SendKeys('print x{ENTER}')
    superConsole.SendKeys('outputRedirectStop{(}{)}{ENTER}')
    
    #verification
    lines = getTestOutput()[0]
    AreEqual(lines[2], '\n')
    AreEqual(lines[5], 'abc\n')

def unverified_raw_input():
    '''
    Intentionally not checking output on this test (based on
    CP14456) as redirecting stdout/stderr will hide the bug.
    '''
    superConsole.SendKeys('x = raw_input{(}"foo:"{)}{ENTER}')
    superConsole.SendKeys('{ENTER}')
#Run this first to corrupt other test cases if it's broken.
unverified_raw_input()
    
#------------------------------------------------------------------------------
#--__main__

try:
    run_test(__name__)
finally:
    # and finally test that F6 shuts it down
    superConsole.SendKeys('{F6}')
    superConsole.SendKeys('{ENTER}')
    sleep(5)
    Assert(not superConsole.IsRunning)