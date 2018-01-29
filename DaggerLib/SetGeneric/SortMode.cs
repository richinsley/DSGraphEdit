
# region Heading

/**************************************************************************************************************/
/*                                                                                                            */
/*  SortMode.cs                                                                                               */
/*                                                                                                            */
/*  An enumeration for specifying if and when to sort something                                               */
/*                                                                                                            */
/*  This is free code, use it as you require. It was a good learning exercise for me and I hope it will be    */
/*  for you too. If you modify it please use your own namespace.                                              */
/*                                                                                                            */
/*  If you like it or have suggestions for improvements please let me know at: PIEBALDconsult@aol.com         */
/*                                                                                                            */
/*  Modification history:                                                                                     */
/*  2005-11-14          Sir John E. Boucher     Created                                                       */
/*                                                                                                            */
/**************************************************************************************************************/

# endregion

namespace PIEBALD.Types
{
    /**
    <summary>
        What type of sorting to perform
    </summary>
    */
    public enum SortMode
    {
        /**
            <summary>
                No sorting
            </summary>
        */
        None
    ,
        /**
            <summary>
                Perform the sort before performing the ToString()s
            </summary>
        */
        Native
    ,
        /**
            <summary>
                Perform the sort after performing the ToString()s
            </summary>
        */
        String
    }
}
