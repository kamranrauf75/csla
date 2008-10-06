Imports System.Reflection
Imports System.ComponentModel

''' <summary>
''' Contains utility methods used by the
''' CSLA .NET framework.
''' </summary>
Public Module Utilities

  ''' <summary>
  ''' Returns a property's type, dealing with
  ''' Nullable(Of T) if necessary.
  ''' </summary>
  ''' <param name="propertyType">Type of the
  ''' property as returned by reflection.</param>
  Public Function GetPropertyType(ByVal propertyType As Type) As Type

    Dim type As Type = propertyType
    If (type.IsGenericType AndAlso _
      (type.GetGenericTypeDefinition Is GetType(Nullable(Of )))) Then

      Return Nullable.GetUnderlyingType(type)
    End If

    Return type

  End Function

  ''' <summary>
  ''' Returns the type of child object
  ''' contained in a collection or list.
  ''' </summary>
  ''' <param name="listType">Type of the list.</param>
  Public Function GetChildItemType(ByVal listType As Type) As Type

    Dim result As Type = Nothing
    If listType.IsArray Then
      result = listType.GetElementType()
    Else
      Dim indexer As DefaultMemberAttribute = _
        CType(Attribute.GetCustomAttribute(listType, _
        GetType(DefaultMemberAttribute)), DefaultMemberAttribute)
      If indexer IsNot Nothing Then
        For Each prop As PropertyInfo In listType.GetProperties( _
          BindingFlags.Public Or _
          BindingFlags.Instance Or _
          BindingFlags.FlattenHierarchy)
          If prop.Name = indexer.MemberName Then
            result = Utilities.GetPropertyType(prop.PropertyType)
          End If
        Next
      End If
    End If
    Return result

  End Function

#Region " CoerceValue "

  ''' <summary>
  ''' Attempts to coerce a value of one type into
  ''' a value of a different type.
  ''' </summary>
  ''' <param name="desiredType">
  ''' Type to which the value should be coerced.
  ''' </param>
  ''' <param name="valueType">
  ''' Original type of the value.
  ''' </param>
  ''' <param name="oldValue">
  ''' The previous value (if any) being replaced by
  ''' the new coerced value.
  ''' </param>
  ''' <param name="value">
  ''' The value to coerce.
  ''' </param>
  ''' <remarks>
  ''' <para>
  ''' If the desired type is a primitive type or Decimal, 
  ''' empty string and null values will result in a 0 
  ''' or equivalent.
  ''' </para>
  ''' <para>
  ''' If the desired type is a Nullable type, empty string
  ''' and null values will result in a null result.
  ''' </para>
  ''' <para>
  ''' If the desired type is an enum the value's ToString()
  ''' result is parsed to convert into the enum value.
  ''' </para>
  ''' </remarks>
  Public Function CoerceValue(ByVal desiredType As Type, ByVal valueType As Type, ByVal oldValue As Object, ByVal value As Object) As Object

    If desiredType.Equals(valueType) Then
      ' types match, just return value
      Return value

    Else
      If desiredType.IsGenericType Then
        If desiredType.GetGenericTypeDefinition() Is GetType(Nullable(Of )) Then
          If value Is Nothing Then
            Return Nothing

          ElseIf valueType.Equals(GetType(String)) AndAlso CStr(value) = String.Empty Then
            Return Nothing
          End If
        End If
        desiredType = Utilities.GetPropertyType(desiredType)
      End If

      If desiredType.IsEnum AndAlso _
          (valueType.Equals(GetType(String)) OrElse _
           [Enum].GetUnderlyingType(desiredType).Equals(valueType)) Then
        Return [Enum].Parse(desiredType, value.ToString())
      End If

      If desiredType.Equals(GetType(SmartDate)) AndAlso oldValue IsNot Nothing Then
        If value Is Nothing Then _
          value = String.Empty
        Dim tmp = DirectCast(oldValue, SmartDate)
        tmp.Text = value.ToString
        Return tmp
      End If

      If (desiredType.IsPrimitive OrElse desiredType.Equals(GetType(Decimal))) _
          AndAlso valueType.Equals(GetType(String)) _
          AndAlso String.IsNullOrEmpty(CStr(value)) Then
        value = 0
      End If

      Try
        If desiredType.Equals(GetType(String)) AndAlso value IsNot Nothing Then
          Return value.ToString()
        Else
          Return Convert.ChangeType(value, desiredType)
        End If
      Catch
        Dim cnv As TypeConverter = TypeDescriptor.GetConverter(desiredType)
        If cnv IsNot Nothing AndAlso cnv.CanConvertFrom(valueType) Then
          Return cnv.ConvertFrom(value)

        Else
          Throw
        End If
      End Try
    End If

  End Function

  ''' <summary>
  ''' Attempts to coerce a value of one type into
  ''' a value of a different type.
  ''' </summary>
  ''' <typeparam name="D">
  ''' Type to which the value should be coerced.
  ''' </typeparam>
  ''' <param name="valueType">
  ''' Original type of the value.
  ''' </param>
  ''' <param name="oldValue">
  ''' The previous value (if any) being replaced by
  ''' the new coerced value.
  ''' </param>
  ''' <param name="value">
  ''' The value to coerce.
  ''' </param>
  ''' <remarks>
  ''' <para>
  ''' If the desired type is a primitive type or Decimal, 
  ''' empty string and null values will result in a 0 
  ''' or equivalent.
  ''' </para>
  ''' <para>
  ''' If the desired type is a Nullable type, empty string
  ''' and null values will result in a null result.
  ''' </para>
  ''' <para>
  ''' If the desired type is an enum the value's ToString()
  ''' result is parsed to convert into the enum value.
  ''' </para>
  ''' </remarks>
  Public Function CoerceValue(Of D)(ByVal valueType As Type, ByVal oldValue As D, ByVal value As Object) As D

    Return DirectCast(CoerceValue(GetType(D), valueType, oldValue, value), D)

  End Function

#End Region

End Module
