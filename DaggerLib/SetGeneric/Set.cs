
# region Heading

/**************************************************************************************************************/
/*                                                                                                            */
/*  Set.cs                                                                                                    */
/*                                                                                                            */
/*  Implements a Set class                                                                                    */
/*                                                                                                            */
/*  This is free code, use it as you require. It was a good learning exercise for me and I hope it will be    */
/*  for you too. If you modify it please use your own namespace.                                              */
/*                                                                                                            */
/*  If you like it or have suggestions for improvements please let me know at: PIEBALDconsult@aol.com         */
/*                                                                                                            */
/*  Modification history:                                                                                     */
/*  2005-07-05          Sir John E. Boucher     Created                                                       */
/*  2006-11-12          Sir John E. Boucher     Fixed some problems, made generic, and added comments         */
/*  2006-11-13          Sir John E. Boucher     Pretty much a complete rewrite                                */
/*  2006-11-15          Sir John E. Boucher     Reworked again to add ability to specify the Comparer         */
/*                                              and ability to specify parameters for ToString()              */
/*                                                                                                            */
/**************************************************************************************************************/

# endregion

// # define Explicit
// # define ThrowOnNull

namespace PIEBALD.Types
{
    /** 
    <summary>
        Represents a Set.
    </summary>
    */
    public partial class Set<T> : System.Collections.IEnumerable
    {
        private System.Collections.Generic.Dictionary<T,object> elements =
            new System.Collections.Generic.Dictionary<T,object>() ;
    
# region Constructor

        /**
        <summary>
            Constructs and populates a Set.
        </summary>
        <param name="Items">
            (Optional) Items to add to the new Set.
        </param>
        */
        public Set
        (
            params object[] Items
        )
        {
            Add ( Items ) ;
        }

# endregion

# region Properties

        /**
        <return>
            The number of elements in the Set.
        </return>
        */
        public virtual int
        Cardinality
        {
            get
            {
                return ( this.elements.Count ) ;
            }
        }

        /**
        <return>
            The System.Collections.Generic.IEqualityComparer to use
        </return>
        */
        public virtual System.Collections.Generic.IEqualityComparer<T>
        EqualityComparer
        {
            get
            {
                return ( this.elements.Comparer ) ;
            }
            
            set
            {
                if ( value != EqualityComparer )
                {
                    System.Collections.IEnumerable temp = this.elements.Keys ;

                    this.elements = new System.Collections.Generic.Dictionary<T,object> ( value ) ;

                    this.Add ( temp ) ;
                }
            }
        }
        
# endregion

# region Conversions

        /**
        <summary>
            Converts an item to a Set
        </summary>
        */
# if Explicit        
        public static explicit operator Set<T>
# else
        public static implicit operator Set<T>
# endif
        (
            T Item
        )
        {
            return ( new Set<T> ( Item ) ) ;
        }

        /**
        <summary>
            Converts an array of items to a Set
        </summary>
        */
# if Explicit
        public static explicit operator Set<T>
# else
        public static implicit operator Set<T>
# endif
        (
            T[] Items
        )
        {
            return ( new Set<T> ( Items ) ) ;
        }

        /**
        <summary>
            Converts an array of items to a Set
        </summary>
        */
# if Explicit
        public static explicit operator Set<T>
# else
        public static implicit operator Set<T>
# endif
        (
            System.Array Items
        )
        {
            return ( new Set<T> ( Items ) ) ;
        }

        /**
        <summary>
            Converts a Collection of items to a Set
        </summary>
        */
# if Explicit
        public static explicit operator Set<T>
# else
        public static implicit operator Set<T>
# endif
        (
            System.Collections.CollectionBase Items
        )
        {
            return ( new Set<T> ( Items ) ) ;
        }

        /**
        <summary>
            Converts an ArrayList of items to a Set
        </summary>
        */
# if Explicit
        public static explicit operator Set<T>
# else
        public static implicit operator Set<T>
# endif
        (
            System.Collections.ArrayList Items
        )
        {
            return ( new Set<T> ( Items ) ) ;
        }

# endregion
    
# region Mathematical Operators

        /**
        <summary>
            Union of the two Sets; Set of items that are elements of at least one of the Sets.
        </summary>
        */
        public static Set<T>
        operator +
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            return ( new Set<T> ( lhs , rhs ) ) ;
        }

        /**
        <summary>
            Relative complement; items that are elements of the first Set, but not Elements of the second Set.
        </summary>
        */
        public static Set<T>
        operator -
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            Set<T> result = new Set<T>() ;
            
            foreach ( T t in lhs )
            {
                if ( !rhs.Contains ( t ) )
                {
                    result.Add ( t ) ;
                }
            }
            
            return ( result ) ;
        }

        /**
        <summary>
            Union of the two Sets; items that are elements of at least one of the Sets.
        </summary>
        */
        public static Set<T>
        operator |
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            return ( lhs + rhs ) ;
        }

        /**
        <summary>
            Intersection of the two Sets; items that are elements of both of the Sets.
        </summary>
        */
        public static Set<T>
        operator &
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            Set<T> result = new Set<T>() ;

            foreach ( T t in lhs )
            {
                if ( rhs.Contains ( t ) )
                {
                    result.Add ( t ) ;
                }
            }

            return ( result ) ;
        }

        /**
        <summary>
            Exclusive Or of the two Sets; items that are elements of only one of the Sets.
        </summary>
        */
        public static Set<T>
        operator ^
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            Set<T> result = new Set<T>() ;

            foreach ( T t in lhs + rhs )
            {
                if ( !lhs.Contains ( t ) || !rhs.Contains ( t ) )
                {
                    result.Add ( t ) ;
                }
            }

            return ( result ) ;
        }

# endregion

# region Comparison Operators

        /**
        <summary>
            Test equality of Sets; True if both Sets have the same elements
        </summary>
        */
        public static bool
        operator ==
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            return ( ( lhs.Cardinality == rhs.Cardinality ) && ( lhs.Contains ( rhs ) ) ) ;
        }

        /**
        <summary>
            Test inequality of Sets; True if the Sets do not have the same elements
        </summary>
        */
        public static bool
        operator !=
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            return ( !( lhs == rhs ) ) ;
        }

        /**
        <summary>
            Subset; true if the first Set is a subset of (but is not equal to) the second
        </summary>
        */
        public static bool
        operator <
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            return ( ( lhs.Cardinality < rhs.Cardinality ) && ( rhs.Contains ( lhs ) ) );
        }

        /**
        <summary>
            Superset; true if the first Set is a superset of (but is not equal to) the second 
        </summary>
        */
        public static bool
        operator >
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            return ( rhs < lhs ) ;
        }

        /**
        <summary>
            Subset; true if the first Set is a subset of the second 
        </summary>
        */
        public static bool
        operator <=
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            return ( ( lhs.Cardinality <= rhs.Cardinality ) && ( rhs.Contains ( lhs ) ) );
        }

        /**
        <summary>
            Superset; true if the first Set is a superset of the second 
        </summary>
        */
        public static bool
        operator >=
        (
            Set<T> lhs
        ,
            Set<T> rhs
        )
        {
            return ( rhs <= lhs ) ;
        }
 
# endregion

# region Overrides

        /**
        <summary>
            Enumerator for the elements of the Set
        </summary>
        */
        public virtual System.Collections.IEnumerator
        GetEnumerator
        (
        )
        {
            return ( this.elements.Keys.GetEnumerator() ) ;
        }
        
        /**
        <summary>
            Yada yada yada
        </summary>
        */
        public override bool
        Equals
        (
            object rhs
        )
        {
            return ( ( rhs.GetType() == typeof ( PIEBALD.Types.Set<T> ) ) && ( this == (Set<T>) rhs ) ) ;
        }

        /**
        <summary>
            Yada yada yada
        </summary>
        */
        public override int
        GetHashCode
        (
        )
        {
            return ( this.elements.GetHashCode() ) ;
        }

        /**
        <summary>
            Returns the elements of the Set in set format; { element1 , element2 ... }
        </summary>
        <remarks>
            ToString() is called on each element in turn.
            No attempt is made to protect against elements whose ToString() values contain commas or braces.
        </remarks>
        */
        public override string
        ToString
        (
        )
        {
            return ( ToString ( SortMode.None ) ) ;
        }

        /**
        <summary>
            Returns the elements of the Set in set format; { element1 , element2 ... }
        </summary>
        <param name="SortMode">
            Whether or not to sort the elements.
        </param>
        <param name="FormatInfo">
            (Optional) Formatting information to pass to ToString()
        </param>
        <remarks>
            ToString() is called on each element in turn.
            No attempt is made to protect against elements whose ToString() values contain commas or braces.
        </remarks>
        */
        public virtual string
        ToString
        (
            SortMode        SortMode
        ,
            params object[] FormatInfo
        )
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder() ;
            System.Reflection.MethodInfo meth = typeof(T).GetMethod 
            ( "ToString" , PIEBALD.Lib.LibSys.GetTypes ( FormatInfo ) ) ;
            
            if ( this.elements.Count > 0 )
            {            
                string sep = "{ " ;
                
                switch ( SortMode )
                {
                    case SortMode.None :
                    {
                        foreach ( T t in this )
                        {
                            result.Append ( sep ) ;
                            result.Append ( (string) meth.Invoke ( t , FormatInfo ) ) ;
                            sep = " , " ;
                        }

                        break ;
                    }
                    
                    case SortMode.Native :
                    {
                        System.Collections.Generic.List<T> temp = 
                            new System.Collections.Generic.List<T> ( this.elements.Keys ) ;
                            
                        temp.Sort() ;

                        foreach ( T t in temp )
                        {
                            result.Append ( sep ) ;
                            result.Append ( (string) meth.Invoke ( t , FormatInfo ) ) ;
                            sep = " , ";
                        }

                        break ;
                    }
                    
                    case SortMode.String :
                    {
                        System.Collections.Generic.List<string> temp = 
                            new System.Collections.Generic.List<string> ( this.Cardinality ) ;

                        foreach ( T t in this )
                        {
                            temp.Add ( (string) meth.Invoke ( t , FormatInfo ) ) ;
                        }

                        temp.Sort() ;

                        foreach ( string s in temp )
                        {
                            result.Append ( sep ) ;
                            result.Append ( s ) ;
                            sep = " , " ;
                        }

                        break ;
                    }
                }
                
                result.Append ( " }" ) ;
            }
            else
            {
                result.Append ( "{}" ) ;
            }

            return ( result.ToString() ) ;
        }

# endregion

# region Operations

        /**
        <summary>
            Attempts to add Items to the Set
        </summary>
        */
        public virtual Set<T>
        Add
        (
            params object[] Items 
        )
        {
            foreach ( object i in Items )
            {
                if ( i is T )
                {
                    if ( !this.elements.ContainsKey ( (T) i ) )
                    {
                        this.elements.Add ( (T) i , null ) ;
                    }
                }
                else
                {
                    if ( i is System.Collections.IEnumerable )
                    {
                        foreach ( object o in (System.Collections.IEnumerable) i )
                        {
                            Add ( o ) ;
                        }
                    }
                    else
                    {
                        if ( i == null )
                        {
/* If ThrowOnNull is defined, nulls in the data will cause an exception to be thrown, otherwise they are ignored */
# if ThrowOnNull
                            throw ( new System.NullReferenceException ( "The Set may not contain null" ) ) ;
# endif
                        }
                        else
                        {
                            throw ( new System.InvalidOperationException ( i.ToString() + " is not a " + typeof(T).ToString() ) ) ;
                        }
                    }
                }
            }
            
            return ( this ) ;
        }

        /**
        <summary>
            Attempts to remove Items from the Set
        </summary>
        */
        public virtual Set<T>
        Remove
        (
            params object[] Items
        )
        {
            foreach ( object i in Items )
            {
                if ( i is T )
                {
                    this.elements.Remove ( (T) i ) ;
                }
                else
                {
                    if ( i is System.Collections.IEnumerable )
                    {
                        foreach ( object o in (System.Collections.IEnumerable) i )
                        {
                            Remove ( o ) ;
                        }
                    }
                }
            }
            
            return ( this ) ;
        }

        /**
        <summary>
            Returns true if the Set contains the Item(s)
        </summary>
        */
        public virtual bool
        Contains
        (
            params object[] Items
        )
        {
            bool result = true ;

            foreach ( object i in Items )
            {
                if ( i is T )
                {
                    if ( !this.elements.ContainsKey ( (T) i ) )
                    {
                        result = false ;
                        
                        break ;
                    }
                }
                else
                {
                    if ( i is System.Collections.IEnumerable )
                    {
                        foreach ( object o in (System.Collections.IEnumerable) i )
                        {
                            if ( !Contains ( o ) )
                            {
                                result = false ;
                                
                                break ;
                            }
                        }
                    }
                }
            }
                                    
            return ( result ) ;
        }

        /**
        <summary>
            Removes all elements from the Set
        </summary>
        */
        public virtual Set<T>
        Clear
        (
        )
        {
            this.elements.Clear() ;
            return ( this ) ;
        }

# endregion

    }
}
