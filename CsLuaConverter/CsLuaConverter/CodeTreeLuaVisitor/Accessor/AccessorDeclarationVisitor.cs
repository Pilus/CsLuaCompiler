﻿namespace CsLuaConverter.CodeTreeLuaVisitor.Accessor
{
    using System.Linq;
    using CodeTree;
    using CsLuaConverter.Context;
    using CsLuaConverter.SyntaxExtensions;
    using Filters;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class AccessorDeclarationVisitor : SyntaxVisitorBase<AccessorDeclarationSyntax>, IAccessor
    {
        public AccessorDeclarationVisitor(CodeTreeBranch branch) : base(branch)
        {

        }

        public AccessorDeclarationVisitor(AccessorDeclarationSyntax syntax) : base(syntax)
        {
            
        }

        public override void Visit(IIndentedTextWriterWrapper textWriter, IContext context)
        {
            this.Syntax.Write(textWriter, context);
        }

        public bool IsAutoProperty()
        {
            throw new System.NotImplementedException();
        }
    }
}