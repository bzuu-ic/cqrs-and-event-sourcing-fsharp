﻿module PinetreeShop.Domain.Orders.Tests.ReserveOrderLine

open PinetreeShop.Domain.Tests.TestBase
open PinetreeShop.Domain.Orders.OrderAggregate
open PinetreeShop.Domain.Orders.Tests.Base
open PinetreeCQRS.Infrastructure.Commands
open PinetreeCQRS.Infrastructure.Events
open PinetreeCQRS.Infrastructure.Types
open Xunit
open System

let aggregateId = Guid.NewGuid() |> AggregateId
let basketId = Guid.NewGuid() |> BasketId

let orderLine = 
    { ProductId = Guid.NewGuid() |> ProductId
      ProductName = "Test"
      Price = 2m
      Quantity = 2 }

let orderLine2 = 
    { ProductId = Guid.NewGuid() |> ProductId
      ProductName = "Test 2"
      Price = 2m
      Quantity = 2 }

[<Theory>]
[<InlineData("Pending", true)>]
[<InlineData("Cancelled", false)>]
[<InlineData("ReadyForShipping", false)>]
[<InlineData("Shipped", false)>]
[<InlineData("Delivered", false)>]
[<InlineData("NotCreated", false)>]
let ``When AddOrderLine not pending fail`` state isSuccess = 
    let initialEvent1 = OrderCreated(basketId, ShippingAddress "a", [ orderLine; orderLine2 ])
    
    let initialEvents = 
        match state with
        | "Pending" -> [ initialEvent1 ]
        | "Cancelled" -> [ initialEvent1; OrderCancelled ]
        | "ReadyForShipping" -> [ initialEvent1; OrderReadyForShipping ]
        | "Shipped" -> [ initialEvent1; OrderShipped ]
        | "Delivered" -> [ initialEvent1; OrderDelivered ]
        | _ -> []
    
    let command = 
        ReserveOrderLineProduct orderLine.ProductId |> createCommand aggregateId (Irrelevant, None, None, None)
    let initialEvents' = List.map (fun e -> createInitialEvent aggregateId 0 e) initialEvents
    let error = sprintf "Wrong Order state %s" state
    
    let checkResult r = 
        match isSuccess with
        | true -> checkSuccess [ createExpectedEvent command 1 (OrderLineProductReserved orderLine.ProductId) ] r
        | false -> checkFailure [ ValidationError error ] r
    handleCommand initialEvents' command |> checkResult

[<Fact>]
let ``When all products reserved ready for shipping``() = 
    let initialEvents = 
        [ OrderCreated(basketId, ShippingAddress "a", [ orderLine; orderLine2 ])
          OrderLineProductReserved orderLine.ProductId ]
    
    let command = 
        ReserveOrderLineProduct orderLine2.ProductId |> createCommand aggregateId (Irrelevant, None, None, None)
    let initialEvents' = List.map (fun e -> createInitialEvent aggregateId 0 e) initialEvents
    
    let expectedEvents = 
        [ OrderLineProductReserved orderLine2.ProductId
          OrderReadyForShipping ]
        |> List.map (createExpectedEvent command 1)
    handleCommand initialEvents' command |> checkSuccess expectedEvents
