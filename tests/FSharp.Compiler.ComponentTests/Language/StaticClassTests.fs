﻿namespace FSharp.Compiler.ComponentTests.Language

open Xunit
open FSharp.Test.Compiler

module StaticClassTests =

    [<Fact>]
    let ``Sealed and AbstractClass on a type in lang version70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T = class end
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed

    [<Fact>]
    let ``Sealed and AbstractClass on a type in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T = class end
        """
         |> withLangVersionPreview
         |> compile
         |> shouldSucceed

    [<Fact>]
    let ``Sealed and AbstractClass on a type with constructor in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() = class end
        """
         |> withLangVersionPreview
         |> compile
         |> shouldSucceed

    [<Fact>]
    let ``Sealed and AbstractClass on a type with constructor in lang version70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() = class end
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed

    [<Fact>]
    let ``Sealed and AbstractClass on a type with constructor with arguments in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T(x: int) = class end
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3552, Line 3, Col 8, Line 3, Col 14, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Constructor with arguments is not allowed.")
         ]

    [<Fact>]
    let ``Sealed and AbstractClass on a type with constructor with arguments in lang version70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T(x: int) = class end
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed

    [<Fact>]
    let ``When Sealed and AbstractClass on a type with additional constructors in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T =
    new () = {}
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3553, Line 4, Col 5, Line 4, Col 16, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Additional constructor is not allowed.")
         ]

    [<Fact>]
    let ``When Sealed and AbstractClass on a type with additional constructors in lang version70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T =
    new () = {}
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed

    [<Fact>]
    let ``When Sealed and AbstractClass on a type with a primary(parameters) and additional constructor in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T(x: int) =
    new () = T(42)
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3552, Line 3, Col 8, Line 3, Col 14, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Constructor with arguments is not allowed.")
             (Error 3553, Line 4, Col 5, Line 4, Col 19, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Additional constructor is not allowed.")
         ]
         
    [<Fact>]
    let ``When Sealed and AbstractClass on a type with explicit fields and constructor in lang version70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type B =
    val F : int
    val mutable G : int
    new () = { F = 3; G = 3 }
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
    [<Fact>]
    let ``When Sealed and AbstractClass on a generic type with constructor in lang version70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type ListDebugView<'T>(l: 'T list) = class end
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``When Sealed and AbstractClass on a generic type with constructor in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type ListDebugView<'T>(l: 'T list) = class end
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3552, Line 3, Col 24, Line 3, Col 34, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Constructor with arguments is not allowed.")
         ]

    [<Fact>]
    let ``When Sealed and AbstractClass on a type with explicit fields and constructor in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type B =
    val F : int
    val mutable G : int
    new () = { F = 3; G = 3 }
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3553, Line 6, Col 5, Line 6, Col 30, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Additional constructor is not allowed.")
             (Error 3558, Line 4, Col 9, Line 4, Col 10, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Explicit field declarations are not allowed.")
             (Error 3558, Line 5, Col 17, Line 5, Col 18, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Explicit field declarations are not allowed.")
         ]

    [<Theory>]
    [<InlineData("preview")>]
    [<InlineData("7.0")>]
    let ``Mutually recursive type definition that using custom attributes``(langVersion) =
        let code = """
        module Test

        open System.Diagnostics

        [<DefaultAugmentation(false)>]
        [<DebuggerTypeProxyAttribute(typedefof<MyCustomListDebugView<_>>)>]
        [<DebuggerDisplay("{DebugDisplay,nq}")>]
        [<CompiledName("MyCustomList")>]
        type MyCustomList<'T> = 
            | Empty
            | NonEmpty of Head: 'T * Tail: MyCustomList<'T>
        
        and MyImbaAlias<'T> = MyCustomList<'T>

        //-------------------------------------------------------------------------
        // List (debug view)
        //-------------------------------------------------------------------------

        and
            MyCustomListDebugView<'T>(l: MyCustomList<'T>) =
                let asList =
                    let rec toList ml = 
                        match ml with
                        | Empty -> []
                        | NonEmpty (head,tail) -> head :: (toList tail)
                    toList l

                [<DebuggerBrowsable(DebuggerBrowsableState.RootHidden)>]
                member x.Items = asList |> List.toArray

                [<DebuggerBrowsable(DebuggerBrowsableState.Collapsed)>]
                member x._FullList = asList |> List.toArray

        """
        Fs code
        |> withLangVersion langVersion
        |> compile
        |> shouldSucceed

    [<Fact>]
    let ``Sealed and AbstractClass on a type with instance members in lang version70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() =
    member this.M() = ()
    static member X = 1
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with instance members in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() =
    member this.M() = ()
    static member X = 1
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3554, Line 4, Col 5, Line 4, Col 25, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Instance members are not allowed.")
         ]
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with static members in lang version70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() =
    static member M() = ()
    static member X = T.M()
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with static members in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() =
    static member M() = ()
    static member X = T.M()
        """
         |> withLangVersionPreview
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with static and non static let bindings in lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type C() =
    let a = 1
    static let x = 1
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with static and non static recursive let bindings in lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type C() =
    let rec a = 1
    static let x = 1
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with static let bindings in lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type C() =
    static let a = 1
    static let x = a
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with recursive static let bindings in lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type C() =
    static let rec a = 1
    static let x = a
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass with static and non static let bindings in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type C() =
    let a = 1
    static let X = 1
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3555, Line 4, Col 5, Line 4, Col 14, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Instance let bindings are not allowed.")
         ]
         
    [<Fact>]
    let ``Sealed and AbstractClass with static and non static recursive let bindings in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type C() =
    let rec a = 1
    static let X = 1
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3555, Line 4, Col 5, Line 4, Col 18, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Instance let bindings are not allowed.")
         ]

    [<Fact>]
    let ``Sealed and AbstractClass with static let bindings in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type C() =
    static let a = 1
    static let X = a
        """
         |> withLangVersionPreview
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass with recursive static let bindings in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type C() =
    static let rec a = 1
    static let X = a
        """
         |> withLangVersionPreview
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type implementing interface in lang 70`` () =
        Fsx """
type MyInterface =
    abstract member M : unit -> unit

[<Sealed; AbstractClass>]
type C() =
    interface MyInterface with
        member this.M() = ()
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type implicit constructor implementing interface in lang 70`` () =
        Fsx """
type MyInterface =
    abstract member M : unit -> unit

[<Sealed; AbstractClass>]
type C =
    interface MyInterface with
        member this.M() = ()
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type implementing interface in lang preview`` () =
        Fsx """
type MyInterface =
    abstract member M : unit -> unit

[<Sealed; AbstractClass>]
type C() =
    interface MyInterface with
        member this.M() = ()
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
            (Error 3556, Line 8, Col 9, Line 8, Col 29, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Implementing interfaces is not allowed.")
         ]
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type implicit constructor implementing interface in lang preview`` () =
        Fsx """
type MyInterface =
    abstract member M : unit -> unit

[<Sealed; AbstractClass>]
type C =
    interface MyInterface with
        member this.M() = ()
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
            (Error 3556, Line 8, Col 9, Line 8, Col 29, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Implementing interfaces is not allowed.")
         ]
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type implicit constructor declaring abstract members in Lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T =
    abstract A : int
    abstract B : int with get, set
    abstract C : i:int -> int
    abstract D : i:int -> int
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
    
    [<Fact>]
    let ``Sealed and AbstractClass on a type declaring abstract members in Lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() =
    abstract A : int
    abstract B : int with get, set
    abstract C : i:int -> int
    abstract D : i:int -> int
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with implicit constructor declaring abstract members in Lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T =
    abstract C : i:int -> int
    abstract D : i:int -> int
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3557, Line 4, Col 14, Line 4, Col 15, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Abstract member declarations are not allowed.")
             (Error 3557, Line 5, Col 14, Line 5, Col 15, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Abstract member declarations are not allowed.")
         ]
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type declaring abstract members in Lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() =
    abstract C : i:int -> int
    abstract D : i:int -> int
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3557, Line 4, Col 14, Line 4, Col 15, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Abstract member declarations are not allowed.")
             (Error 3557, Line 5, Col 14, Line 5, Col 15, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Abstract member declarations are not allowed.")
         ]

    #if !NETCOREAPP
    [<Fact(Skip = "IWSAMs are not supported by NET472.")>]
    #else
    [<Fact>]
    #endif
    let ``Sealed and AbstractClass on a type implementing an interface with static abstract members in Lang 70`` () =
        Fsx """
[<Interface>]
type InputRetriever<'T when 'T:>InputRetriever<'T>> =
    static abstract Read: unit -> string

[<AbstractClass;Sealed>]
type ConsoleRetriever = 
    interface InputRetriever<ConsoleRetriever> with
        static member Read() = 
            stdout.WriteLine("Please enter a value and press enter")
            stdin.ReadLine()
        """
         |> withNoWarn 3535
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    #if !NETCOREAPP
    [<Fact(Skip = "IWSAMs are not supported by NET472.")>]
    #else
    [<Fact>]
    #endif
    let ``Sealed and AbstractClass on a type implicit constructor implementing an interface with static abstract members in Lang preview`` () =
        Fsx """
[<Interface>]
type InputRetriever<'T when 'T:>InputRetriever<'T>> =
    static abstract Read: unit -> string

[<AbstractClass;Sealed>]
type ConsoleRetriever = 
    interface InputRetriever<ConsoleRetriever> with
        static member Read() = 
            stdout.WriteLine("Please enter a value and press enter")
            stdin.ReadLine()
        """
         |> withNoWarn 3535
         |> withLangVersionPreview
         |> compile
         |> shouldSucceed
         
    #if !NETCOREAPP
    [<Fact(Skip = "IWSAMs are not supported by NET472.")>]
    #else
    [<Fact>]
    #endif
    let ``Sealed and AbstractClass on a type implementing an interface with static abstract members in Lang preview`` () =
        Fsx """
[<Interface>]
type InputRetriever<'T when 'T:>InputRetriever<'T>> =
    static abstract Read: unit -> string

[<AbstractClass;Sealed>]
type ConsoleRetriever() = 
    interface InputRetriever<ConsoleRetriever> with
        static member Read() = 
            stdout.WriteLine("Please enter a value and press enter")
            stdin.ReadLine()
        """
         |> withNoWarn 3535
         |> withLangVersionPreview
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with implicit constructor declaring static explicit field in Lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T =
    [<DefaultValue>]
    static val mutable private F : int
    [<DefaultValue>]
    static val mutable private G : int
    
    static member Inc() = T.F <- T.F + 1
    
    static member Get() = T.F
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with declaring static explicit field in Lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() =
    [<DefaultValue>]
    static val mutable private F : int
    [<DefaultValue>]
    static val mutable private G : int
    
    static member Inc() = T.F <- T.F + 1
    
    static member Get() = T.F
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with implicit constructor declaring static explicit field in Lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T =
    [<DefaultValue>]
    static val mutable private F : int
    [<DefaultValue>]
    static val mutable private G : int
    
    static member Inc() = T.F <- T.F + 1
    
    static member Get() = T.F
        """
         |> withLangVersionPreview
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``Sealed and AbstractClass on a type with declaring static explicit field in Lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type T() =
    [<DefaultValue>]
    static val mutable private F : int
    [<DefaultValue>]
    static val mutable private G : int
    
    static member Inc() = T.F <- T.F + 1
    
    static member Get() = T.F
        """
         |> withLangVersionPreview
         |> compile
         |> shouldSucceed
         
    [<Fact>]
    let ``When Sealed and AbstractClass on a type with non static explicit fields and implicit constructor in lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type B =
    val F : int
    val mutable G : int
        """
         |> withLangVersion70
         |> compile
         |> shouldSucceed

    [<Fact>]
    let ``When Sealed and AbstractClass on a type with non static explicit fields and constructor in lang 70`` () =
        Fsx """
[<Sealed; AbstractClass>]
type B() =
    val F : int
    val mutable G : int
        """
         |> withLangVersion70
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 880, Line 4, Col 9, Line 4, Col 16, "Uninitialized 'val' fields must be mutable and marked with the '[<DefaultValue>]' attribute. Consider using a 'let' binding instead of a 'val' field.")
             (Error 880, Line 5, Col 17, Line 5, Col 24, "Uninitialized 'val' fields must be mutable and marked with the '[<DefaultValue>]' attribute. Consider using a 'let' binding instead of a 'val' field.")
         ]

    [<Fact>]
    let ``When Sealed and AbstractClass on a type with non static explicit fields and implicit constructor in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type B =
    val F : int
    val mutable G : int
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 3558, Line 4, Col 9, Line 4, Col 10, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Explicit field declarations are not allowed.")
             (Error 3558, Line 5, Col 17, Line 5, Col 18, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Explicit field declarations are not allowed.")
         ]
         
    [<Fact>]
    let ``When Sealed and AbstractClass on a type with non static explicit fields and constructor in lang preview`` () =
        Fsx """
[<Sealed; AbstractClass>]
type B() =
    val F : int
    val mutable G : int
        """
         |> withLangVersionPreview
         |> compile
         |> shouldFail
         |> withDiagnostics [
             (Error 880, Line 4, Col 9, Line 4, Col 16, "Uninitialized 'val' fields must be mutable and marked with the '[<DefaultValue>]' attribute. Consider using a 'let' binding instead of a 'val' field.")
             (Error 880, Line 5, Col 17, Line 5, Col 24, "Uninitialized 'val' fields must be mutable and marked with the '[<DefaultValue>]' attribute. Consider using a 'let' binding instead of a 'val' field.")
             (Error 3558, Line 4, Col 9, Line 4, Col 10, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Explicit field declarations are not allowed.")
             (Error 3558, Line 5, Col 17, Line 5, Col 18, "If a type uses both [<Sealed>] and [<AbstractClass>] attributes, it means it is static. Explicit field declarations are not allowed.")
         ]

