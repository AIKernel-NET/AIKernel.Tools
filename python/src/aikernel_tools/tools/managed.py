from __future__ import annotations

from typing import Mapping

from aikernel_tools.native import load_tools_runtime


class ManagedObject:
    """[EN]
    Base wrapper for public C# Tools objects.

    [JA]
    公開 C# Tools object の基底 wrapper です。
    """

    def __init__(self, managed):
        self._managed = managed

    @property
    def managed(self):
        """[EN] Return the underlying C# object.

        [JA] 背後の C# object を返します。
        """
        return self._managed

    def to_managed(self):
        """[EN] Return the underlying C# object for contract calls.

        [JA] 契約呼び出し用に背後の C# object を返します。
        """
        return self._managed


def to_dictionary(values: Mapping[str, str]):
    """[EN]
    Convert a Python mapping to System.Collections.Generic.Dictionary.

    [JA]
    Python mapping を System.Collections.Generic.Dictionary に変換します。
    """
    load_tools_runtime()
    from System.Collections.Generic import Dictionary  # type: ignore[import-not-found]

    dictionary = Dictionary[str, str]()
    for key, value in values.items():
        dictionary[str(key)] = str(value)
    return dictionary


def to_string_list(values):
    """[EN]
    Convert Python values to System.Collections.Generic.List[str].

    [JA]
    Python values を System.Collections.Generic.List[str] に変換します。
    """
    load_tools_runtime()
    from System.Collections.Generic import List  # type: ignore[import-not-found]

    items = List[str]()
    for value in values:
        items.Add(str(value))
    return items


def managed_type(type_name: str, assembly_name: str):
    """[EN]
    Resolve a C# type by assembly-qualified name.

    [JA]
    assembly-qualified name で C# type を解決します。
    """
    load_tools_runtime()
    from System import Type  # type: ignore[import-not-found]

    resolved = Type.GetType(f"{type_name}, {assembly_name}")
    if resolved is None:
        raise RuntimeError(f"Unable to resolve managed type: {type_name}, {assembly_name}")
    return resolved


def create_managed(type_name: str, assembly_name: str, *args):
    """[EN]
    Create a managed object through reflection.

    [JA]
    reflection 経由で managed object を作成します。
    """
    constructors = managed_type(type_name, assembly_name).GetConstructors()
    if len(constructors) == 0:
        raise RuntimeError(f"Unable to resolve managed constructor: {type_name}")
    return constructors[0].Invoke(_object_array(args))


def call_static(type_name: str, assembly_name: str, method_name: str, *args):
    """[EN]
    Invoke a public static C# method through reflection.

    [JA]
    公開 static C# method を reflection 経由で呼び出します。
    """
    method = managed_type(type_name, assembly_name).GetMethod(method_name)
    if method is None:
        raise RuntimeError(f"Unable to resolve managed method: {type_name}.{method_name}")
    return method.Invoke(None, _object_array(args))


def _object_array(values):
    load_tools_runtime()
    from System import Array, Object  # type: ignore[import-not-found]

    items = Array[Object](len(values))
    for index, value in enumerate(values):
        items[index] = value
    return items
