
# region Heading

/**************************************************************************************************************/
/*                                                                                                            */
/*  GetTypes.cs                                                                                               */
/*                                                                                                            */
/*  Takes an IEnumerable and returns a System.Type[]                                                          */
/*                                                                                                            */
/*  This is free code, use it as you require. It was a good learning exercise for me and I hope it will be    */
/*  for you too. If you modify it please use your own namespace.                                              */
/*                                                                                                            */
/*  If you like it or have suggestions for improvements please let me know at: PIEBALDconsult@aol.com         */
/*                                                                                                            */
/*  Modification history:                                                                                     */
/*  2006-11-15          Sir John E. Boucher     Created                                                       */
/*                                                                                                            */
/**************************************************************************************************************/

# endregion

namespace PIEBALD.Lib
{
    public partial class LibSys
    {
        /**
            <summary>
                Takes an IEnumerable and returns a System.Type[]
            </summary>
            <param name="Items">
                The items whose Types you want
            </param>
            <returns>
                An array containing the System.Types of the Items
            </returns>
        */
        public static System.Type[]
        GetTypes
        (
            System.Collections.IEnumerable Items
        )
        {
            System.Collections.Generic.List<System.Type> result = 
                new System.Collections.Generic.List<System.Type>() ;

            foreach ( object o in Items )
            {
                result.Add ( o.GetType() ) ;
            }
            
            return ( result.ToArray() ) ;
        }
    }
}
