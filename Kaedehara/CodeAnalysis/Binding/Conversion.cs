using System.Net.Sockets;
using Kaedehara.CodeAnalysis.Symbols;

namespace Kaedehara.CodeAnalysis.Binding
{
    internal sealed class Conversion{
        
        public static readonly Conversion None = new Conversion(exists:false,isIdentity:false,isImplicit:false);
        public static readonly Conversion Identity = new Conversion(exists:true,isIdentity:true,isImplicit:true);
        public static readonly Conversion Implicit = new Conversion(exists:true,isIdentity:false,isImplicit:true);
        public static readonly Conversion Explicit = new Conversion(exists:true,isIdentity:false,isImplicit:false);


        private Conversion(bool exists,bool isIdentity,bool isImplicit){
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exists && !IsImplicit;


        public static Conversion Classify(TypeSymbol from,TypeSymbol to)
        {
            if (from == to){
                return Conversion.Identity;
            }
            if (from == TypeSymbol.Int || 
                from == TypeSymbol.Bool ||
                from == TypeSymbol.Float ||
                from == TypeSymbol.Char){
                    if(to == TypeSymbol.String){
                        return Conversion.Explicit;
                    }
                }
             if (from == TypeSymbol.String){
                    if(to == TypeSymbol.Bool || to == TypeSymbol.Int || to == TypeSymbol.Float || to == TypeSymbol.Char){
                        return Conversion.Explicit;
                    }
                }
             if (from == TypeSymbol.Int && to == TypeSymbol.Float){
                 return Conversion.Implicit;
             }
             if (from == TypeSymbol.Float && to == TypeSymbol.Int){
                 return Conversion.Explicit;
             }
             if (from == TypeSymbol.Int && to == TypeSymbol.Char){
                 return Conversion.Explicit;
             }
             if (from == TypeSymbol.Char && to == TypeSymbol.Int){
                 return Conversion.Explicit;
             }
            return Conversion.None;
        }
    }


}


