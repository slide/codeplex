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


add_clr_assemblies("loadorder_1c")

# public class NamespaceOrType<T> {
#     public static string Flag = typeof(NamespaceOrType<>).FullName;
# }

import NamespaceOrType

add_clr_assemblies("loadorder_1a")

# namespace NamespaceOrType {
#     public class C {
#         public static string Flag = typeof(C).FullName;
#     }
# }

AreEqual(NamespaceOrType[int].Flag, "NamespaceOrType`1")

import NamespaceOrType

AssertError(TypeError, lambda: NamespaceOrType[int])    # indexing Namespace by type results in TypeError, expected string or SymbolId
AreEqual(NamespaceOrType.C.Flag, "NamespaceOrType.C")