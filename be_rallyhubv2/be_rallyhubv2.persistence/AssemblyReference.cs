using System.Reflection;
using be_rallyhubv2.domain.entities;

namespace be_rallyhubv2.persistence;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}