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
if not is_cli:
    #bail
    from sys import exit
    exit(0)

from System.IO          import Directory
from System             import Environment
from System.IO          import File
from Microsoft.CSharp          import CSharpCodeProvider
from System.CodeDom.Compiler   import CompilerParameters

#------------------------------------------------------------------------------
#--GLOBALS

ORIG_DIR = testpath.public_testdir
IP_DIR = sys.prefix
DLLS_DIR = IP_DIR + "\\DLLs"
PROVIDER = CSharpCodeProvider()

#------------------
#a fairly trivial C# file where we control the name of the namespace
#and the static "BAR" member
cs_ipy = '''
using System;
using System.Collections.Generic;
using System.Text;

namespace foo%s {   
    public class Foo {
        public static int BAR = %d;
        
        public static void Main() {
          //no-op
        }
    }
}
'''
#------------------
#an attempt to hijack a native module provided by IPY
cs_native = '''
using System;
using System.Collections.Generic;
using System.Text;

class sys {
        public static string winver = "HIJACKED";
}
'''

#------------------
#another attempt to hijack a native module provided by IPY
cs_native_re = '''
using System;
using System.Collections.Generic;
using System.Text;

class re {
        public static string compile = "HIJACKED";
}
'''

#------------------
#Text file we'll try to get ipy to load.
garbage = '''
This is a fake DLL.
It's just in place to try to break ipy.exe
'''

#------------------------------------------------------------------------------
#--FUNCTIONS
def compileAssembly(file_name):
    '''
    Helper function compiles a *.cs file.
    '''
    cp = CompilerParameters()
    cp.GenerateExecutable = False
    cp.OutputAssembly = file_name.split(".cs")[0] + ".dll"
    cp.GenerateInMemory = False
    cp.TreatWarningsAsErrors = False
    cp.IncludeDebugInformation = True
    cp.ReferencedAssemblies.Add("IronPython.dll")
    cr = PROVIDER.CompileAssemblyFromFile(cp, file_name)
    
def compileExe(file_name):
    '''
    Helper function compiles a *.cs file.
    '''
    cp = CompilerParameters()
    cp.GenerateExecutable = True
    cp.OutputAssembly = file_name.split(".cs")[0] + ".exe"
    cp.GenerateInMemory = False
    cp.TreatWarningsAsErrors = False
    cp.IncludeDebugInformation = True
    cp.ReferencedAssemblies.Add("IronPython.dll")
    cr = PROVIDER.CompileAssemblyFromFile(cp, file_name)


def createAssembly(file_id, namespace_id, bar_num, default_filename="foo"):
    '''
    Helper function creates a single "foo" assembly. Returns
    the file_name.
    '''
    #create the C# file
    file_name = Directory.GetCurrentDirectory() + "\\" + default_filename + str(file_id) + ".cs"
    file = open(file_name, "w")
    print >> file, cs_ipy % (str(namespace_id), 
                              bar_num)
    file.close()
    
    #create the assembly
    compileAssembly(file_name)
    
    
def uniqueDLLNames():
    '''
    Creates eccentric DLL names to ensure IP still loads them
    '''
    createAssembly("", "ZERO", 1, default_filename="0")
    createAssembly("", "ONE", 1, default_filename="1")
    createAssembly("", "a", 1, default_filename="a")
    createAssembly("", "UNDERSCORE", 1, default_filename="_")
    createAssembly("", "WHITESPACE", 1, default_filename="a A")
    
    temp = ""
    for i in xrange(0, 20):
        temp = temp + "aaaaaaaaaa"
    createAssembly("", "BIGFILENAME", 1, default_filename=temp)   
    
    
def textFiles():
    '''
    Creates *.txt files. One is actually a text file
    and the other is a DLL in disguise
    '''
    #create a fake DLL
    file_name = Directory.GetCurrentDirectory() + "\\fooGARBAGE.dll"
    file = open(file_name, "w")
    print >> file, garbage
    file.close()
    
    #create a real DLL and give it the *.txt extension
    createAssembly("TXTDLL", "TXTDLL", 7)
    
    File.Move("fooTXTDLL.dll", "fooTXTDLL.txt")
    
def exeOnly():
    '''
    Creates an EXE in it's own namespace.
    '''
    #create an EXE C# file
    file_name = Directory.GetCurrentDirectory() + "\\fooEXEONLY.cs"
    file = open(file_name, "w")
    print >> file, cs_ipy % ("EXEONLY", 
                              100)
    file.close()
    
    #create the exe
    compileExe(file_name)
    
def dllVsExe():
    '''
    Creates a DLL and EXE of the same namespace to see
    which one IP uses.
    '''
    #create the DLL C# file
    createAssembly("DLL", "DLLEXE", 1)
    #create the assembly
    compileAssembly(Directory.GetCurrentDirectory() + "\\fooDLL.cs")
    
    #create an EXE C# file
    file_name = Directory.GetCurrentDirectory() + "\\fooEXE.cs"
    file = open(file_name, "w")
    print >> file, cs_ipy % ("DLLEXE", 
                              0)
    file.close()    
    #create the exe
    compileExe(file_name)
    
    
def okAssemblies(num):
    '''
    Creates a number, num, of normal assemblies
    that should be loadable by IP.
    '''
    for i in xrange(num): createAssembly(i, i, i)
    
    
def dupAssemblies(num):
    '''
    Creates assemplies which are basically
    duplicates of each other except that foo.FOO.BAR has 
    unique values. IP
    '''
    for i in xrange(num): createAssembly("DUP" + str(i), 
                                          "", 
                                          i)
    
    
def overrideNative():
    '''
    Tries to override the native IP sys module.
    '''
    #create the "sys" C# file
    file_name = Directory.GetCurrentDirectory() + "\\sys.cs"
    file = open(file_name, "w")
    print >> file, cs_native
    file.close()
    
    #compile the assembly
    compileAssembly(file_name)
    
    #create the "re" C# file
    file_name = Directory.GetCurrentDirectory() + "\\re.cs"
    file = open(file_name, "w")
    print >> file, cs_native_re
    file.close()
    
    #compile the assembly
    compileAssembly(file_name)
    

def corruptDLL():
    '''
    Places a corrupt copy of IronPython.dll in DLLs.
    '''
    #get the contents of the assembly
    createAssembly("CORRUPT", "CORRUPT", -1)
    file = open("fooCORRUPT.dll", "rb")
    joe  = file.readlines()
    file.close()
    
    #inject our text to corrupt it
    joe.insert(len(joe)/2, "File is now corrupted...\n")
    
    #rewrite the file to disk
    file = open("fooCORRUPT.dll", "wb")
    file.writelines(joe)
    file.close()

 
def unmanagedDLL():
    '''
    Places an unmanaged DLL inside DLLs.
    '''
    twain = Environment.GetEnvironmentVariable("SystemRoot") + "\\twain.dll"
    File.Copy(twain,
              DLLS_DIR + "\\twain.dll")
              
              
def cleanUp():
    '''
    Just removes the DLLs directory we created.
    '''
    #the following while loop is necessary as
    #the Delete call fails (ipy.exe subprocess has
    #not really released some files yet).
    while 1:
        try:
            Directory.Delete(DLLS_DIR, True)
            break
        except:
            from time import sleep
            sleep(1)
            continue


def setUp():
    '''
    Sets up the DLLs directory.
    '''
    #if it exists, we cannot continue because we will 
    #not have the correct permissions to move/remove the
    #DLLs directory
    if Directory.Exists(DLLS_DIR):
        print "ERROR - cannot test anything with a pre-existing DLLs directory"
        raise Exception("Cannot move/delete DLLs which is being used by this process!")
    
    Directory.CreateDirectory(DLLS_DIR)
    
    File.Copy(IP_DIR + "\\IronPython.dll",
              DLLS_DIR + "\\IronPython.dll")
    
    Directory.SetCurrentDirectory(DLLS_DIR)
    
    
    #create a number of "normal" assemblies
    okAssemblies(50)
    
    #create 5 assemblies in the fooFIVE namespace with nearly
    #identical contents
    dupAssemblies(5)
    
    #recreate the sys module
    overrideNative()
    
    #ensure corrupt DLLs cannot work
    corruptDLL()
    
    #ensure unmanaged DLLs don't break it
    unmanagedDLL()

    #create an exe and a DLL competing for the same
    #namespace
    dllVsExe()
    
    #create an exe in it's own namespace
    exeOnly()
    
    #create a DLL that's really a *.txt and
    #create a *.txt that's really a DLL
    textFiles()
    
    #creates "unique" DLL names that should be loadable
    #by IP
    uniqueDLLNames()
    
    #cleanup after ourselves
    File.Delete(DLLS_DIR + "\\IronPython.dll")


def main():
    '''
    Runs the test by spawning off another IP process which 
    utilizes the newly created DLLs directory.
    '''
    setUp()
    
    Directory.SetCurrentDirectory(ORIG_DIR)
    
    from lib.process_util import launch_ironpython
    launch_ironpython("dllsite.py", "OKtoRun")

    cleanUp()
    
    
if __name__=="__main__":
    main()