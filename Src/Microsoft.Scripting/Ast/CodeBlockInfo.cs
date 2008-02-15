/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    class CodeBlockInfo {
        /// <summary>
        /// The CodeBlock to which this info belongs.
        /// </summary>
        private readonly CodeBlock _block;

        /// <summary>
        /// Variables referenced from this code block.
        /// </summary>
        private readonly Dictionary<Variable, VariableReference> _references = new Dictionary<Variable, VariableReference>();

        /// <summary>
        /// Try statements in this block (if the block is generator)
        /// </summary>
        private Dictionary<TryStatement, TryStatementInfo> _tryInfos;

        /// <summary>
        /// The factory to create the environment (if the block has environment)
        /// </summary>
        private EnvironmentFactory _environmentFactory;

        /// <summary>
        /// The block is a closure (it references variables outside its own scope)
        /// </summary>
        private bool _isClosure;

        /// <summary>
        /// The block has environment
        /// Either its variables are referenced from nested scopes, or the block
        /// is a generator, or outputs its locals into an enviroment altogether
        /// </summary>
        private bool _hasEnvironment;

        /// <summary>
        /// The count of generator temps required to generate the block
        /// (if the block is a generator)
        /// </summary>
        private int _generatorTemps;

        /// <summary>
        /// The top targets for the generator dispatch.
        /// (if the block is a generator)
        /// </summary>
        private IList<YieldTarget> _topTargets;

        internal CodeBlockInfo(CodeBlock block) {
            _block = block;
        }

        internal CodeBlock CodeBlock {
            get { return _block; }
        }

        internal Dictionary<Variable, VariableReference> References {
            get { return _references; }
        }

        internal EnvironmentFactory EnvironmentFactory {
            get { return _environmentFactory; }
            set { _environmentFactory = value; }
        }

        /// <summary>
        /// The method refers to a variable in one of its parents lexical context and will need an environment
        /// flown into it.  A function which is a closure does not necessarily contain an Environment unless
        /// it contains additional closures or uses language features which require lifting all locals to
        /// an environment.
        /// </summary>
        internal bool IsClosure {
            get { return _isClosure; }
            set { _isClosure = value; }
        }

        /// <summary>
        /// Scopes with environments will have some locals stored within a dictionary (FunctionEnvironment).  If
        /// we are also a closure an environment is flown into the method and our environment will point to the
        /// parent environment.  Ultimately this will enable our children to get at our or our parents envs.
        /// 
        /// Upon entering a function with an environment a new CodeContext will be allocated with a new
        /// FunctionEnviroment as its locals.  In the case of a generator this new CodeContext and environment
        /// is allocated in the function called to create the Generator, not the function that implements the
        /// Generator body.
        /// 
        /// The environment is provided as the Locals of a CodeContext or in the case of a Generator 
        /// as the parentEnvironment field.
        /// </summary>
        internal bool HasEnvironment {
            get { return _hasEnvironment; }
            set { _hasEnvironment = value; }
        }

        protected internal int GeneratorTemps {
            get { return _generatorTemps; }
        }

        internal IList<YieldTarget> TopTargets {
            get { return _topTargets; }
        }

        internal void Reference(Variable variable) {
            if (!_references.ContainsKey(variable)) {
                _references[variable] = new VariableReference(variable);
            }
        }

        internal void AddGeneratorTemps(int count) {
            _generatorTemps += count;
        }

        internal void PopulateGeneratorInfo(Dictionary<TryStatement, TryStatementInfo> infos, List<YieldTarget> topTargets, int temps) {
            _tryInfos = infos;
            _topTargets = topTargets;
            AddGeneratorTemps(temps);
        }

        internal TryStatementInfo TryGetTsi(TryStatement ts) {
            TryStatementInfo tsi;
            if (_tryInfos != null && _tryInfos.TryGetValue(ts, out tsi)) {
                return tsi;
            } else {
                return null;
            }
        }
    }
}
