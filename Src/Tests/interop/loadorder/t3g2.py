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

import clr
clr.AddReference("loadorder_3")

# namespace First {
#     public class Generic1<K, V> {
#         public static string Flag = typeof(Generic1<,>).FullName;
#     }
# }

import First
from First import Generic1

AreEqual(First.Generic1[int, int].Flag, "First.Generic1`2")
AreEqual(Generic1[int, int].Flag, "First.Generic1`2")              

clr.AddReference("loadorder_3g")

# namespace First {
#     public class Generic1<K, V> {
#         public static string Flag = typeof(Generic1<,>).FullName + "_Same";
#     }
# }

AreEqual(First.Generic1[int, int].Flag, "First.Generic1`2_Same")    
AreEqual(Generic1[int, int].Flag, "First.Generic1`2")          

import First

AreEqual(First.Generic1[int, int].Flag, "First.Generic1`2_Same")   

from First import * 

AreEqual(Generic1[int, int].Flag, "First.Generic1`2_Same")         
