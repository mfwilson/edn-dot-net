﻿//   Copyright (c) Thortech Solutions, LLC. All rights reserved.
//   The use and distribution terms for this software are covered by the
//   Eclipse Public License 1.0 (http://opensource.org/licenses/eclipse-1.0.php)
//   which can be found in the file epl-v10.html at the root of this distribution.
//   By using this software in any fashion, you are agreeing to be bound by
//   the terms of this license.
//   You must not remove this notice, or any other, from this software.
//
//   Authors: Mark Perrotta, Dimitrios Kapsalis
//

namespace EDNReaderWriter
module TypeHandlers =
    open EDNReaderWriter.EDNParserTypes
    open EDNTypes

    type public ITypeHandler = 
        interface
            abstract member handleValue: EDNValueParsed -> System.Object
            abstract member handleTag: (QualifiedSymbol * EDNValueParsed) -> System.Object
        end

    type public BaseTypeHandler() = 
        interface ITypeHandler with
            override this.handleValue value = 
                this.handleValue value
            override this.handleTag tagAndValue =
                this.handleTag tagAndValue
        abstract member handleValue: EDNValueParsed -> System.Object
        default this.handleValue(value) =
            try
                match value.ednValue with
                | EDNNil -> null

                | EDNInteger bigInt -> 
                    if bigInt.CompareTo(System.Int64.MaxValue) <= 0 then
                        (System.Numerics.BigInteger.op_Explicit(bigInt) : int64) :> System.Object
                    else
                        bigInt :> System.Object

                | EDNString s -> s :> System.Object

                | EDNBoolean b -> b :> System.Object

                | EDNCharacter  c -> c :> System.Object
 
                | EDNFloat f -> f :> System.Object

                | EDNDecimal d -> d :> System.Object

                | EDNList l -> new EDNList (Seq.filter isNotCommentOrDiscard l |> Seq.map this.handleValue) :> System.Object

                | EDNVector v -> new EDNVector (Array.filter isNotCommentOrDiscard v |> Array.map this.handleValue) :> System.Object

                | EDNSet l -> 
                    let filteredSeq = Seq.filter isNotCommentOrDiscard l
                    let newSeq = Seq.map (fun v -> (this.handleValue v)) filteredSeq
                    new EDNSet(newSeq) :> System.Object

                | EDNMap l -> 
                    let newSeq = Seq.filter isNotCommentOrDiscard l
                                    |> Seq.map (fun v -> (this.handleValue v))
                    new EDNMap(newSeq) :> System.Object

                | EDNSymbol s -> new EDNSymbol(s.prefix, s.name) :> System.Object

                | EDNKeyword k -> new EDNKeyword(k.prefix, k.name) :> System.Object

                | EDNTaggedValue (s, v) -> this.handleTag (s, v)

                | _ -> raise (System.Exception("Not an EDNValue " + value.ednValue.ToString()))
            with
            | :? EDNException as ex -> raise ex
            | ex -> raise (EDNException(ex.Message + " " + getLineColString value))
        abstract member handleTag: (QualifiedSymbol * EDNValueParsed) -> System.Object
        default this.handleTag(tagAndValue) =
            let tag, value = tagAndValue
            match tag.name with
            | "uuid" when tag.prefix = null -> 
                match value.ednValue with  
                | EDNString uuidString -> new System.Guid(uuidString) :> System.Object
                | _ -> raise (System.Exception (sprintf "%A is not a valid uuid." value.ednValue))
            | "inst" when tag.prefix = null ->
                match value.ednValue with
                | EDNString dateString -> System.DateTime.Parse(dateString) :> System.Object
                | _ -> raise (System.Exception (sprintf "%A is not a valid inst." value.ednValue))
            | _ -> this.handleValue  value
    