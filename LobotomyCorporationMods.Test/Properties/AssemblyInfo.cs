// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using Xunit;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties

// Causes all unit tests to run sequentially instead of in parallel.
// Most of our tests touch global static instance objects, so this prevents them from affecting each other.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

[assembly: Guid("c5f8d1d8-5f4e-40d6-b66b-1e2bdf84176b")]
