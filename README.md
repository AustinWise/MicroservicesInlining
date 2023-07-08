# Microservices Inlining Experiments

In Microservices Architecture, different parts of large application are
executed in different logical domains (virtual machines, containers, etc.).
Typically this separation of components of an application is driven by technical
reasons (different languages are more suited to solve certain problems), scale
(different parts of an application have more demand than others), or organizational
(different terms are responsible for different parts of an application).

There are downsides to microservices architecture. Other teams deployments can
break your microservice. Testing you service becomes more complex, because you
have to consider different versions of the services you connect to. It may be
more difficult to efficently scale make small services than a few large ones.
Connections between components are subject to service discovery and timeouts,
instead of simple method calls.

In cases where have separate microservices no longer makes sense, this project
aims to look at ways to get the benefits of a monolith without radically
rearchitecturing an application.

## Microservices Inline

In a native code context, a linker takes object files,
potentially the result of compiling different languages, and merges them into a
single executable program. The goal of these experiments is to do something
similar for a collection of microservices that depend upon each other.

## Goals

Ideally the code of the microservices client are server will execute in the same
address spaces. Message will be passed using shared memory.

If for some reason that is not feasible, it is ok for messages to be passed
using serialization and deserialzation. It might even be worthwhile to consider
models where messages are passed between different processes, but on the same
computer.

Initially these scenarioes will be prototyped using gRPC and C#. We use gRPC
because it has a lot of tooling and frameworks for formally diffing the contents
of messages and for hooking into the message sending process. And C#, because
I'm familiar with it and it's ability for dynamica code loading. Dynamic code
loading (and in the extreme case, IL rewriting) could potentially allow inlining
microservices without recompiling the applications.

## Status

The current projects show some very basic inlining. The examples don't yet try
to make exceptions and timeouts appear the same to clients of an inlined service
as they do to a conventional service.

## TODO

* Credit the insirpation of microservices inlining to the person who coined the
  phrase.
* Make the gRPC prototype properly handle exceptions and timeouts and metadata.
* Support cases where the client and server have independently defined interfaces
  and datastructures. Currently we require our client and server to share the
  same .NET type definitions for requests and responses.
* Take into the account that services are being executed in. Currently we
  require the service to be explicitly constructed. Ideally it would use its own
  dependency injection settings to create itself.

Some ideas on how to make this thing more automatic:

* Support for pointing your ASP.NET app at a services and inlining it's services.
* Support for an external tool to take two or more services and link them
  together, without changing any of their internal code.
