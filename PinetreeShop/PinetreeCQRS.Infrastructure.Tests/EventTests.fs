﻿module PinetreeCQRS.Infrastructure.Tests.EventTests

open PinetreeCQRS.Infrastructure.Types
open System
open Xunit
open PinetreeCQRS.Infrastructure.Events

type TestEvent = 
    | ToLower of string
    | ToUpper of string
    | NotInteresting of int

let handler event = 
    match event with
    | ToUpper e -> e.ToUpper()
    | ToLower e -> e.ToLower()
    | _ -> "not interested"

let events = 
    [ ToLower("Lower")
      ToUpper("Upper")
      NotInteresting(2) ]

let loadEvents lastEventNumber = 
    let aggregateId = Guid.NewGuid()
    let guid = Guid.NewGuid()
    let evt e = createEvent aggregateId e guid (Some(guid)) guid
    List.map evt events

let handleEvents = readAndHandleEvents loadEvents handler

[<Fact>]
let ``When Handling Events Results Are Returned``() = 
    let actual = handleEvents 0
    let expected = ([ "lower"; "UPPER"; "not interested" ], 0)
    Assert.Equal(expected, actual)