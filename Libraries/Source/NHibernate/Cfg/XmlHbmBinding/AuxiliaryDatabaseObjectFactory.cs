using System;
using System.Collections.Generic;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping;
using NHibernate.Util;

namespace NHibernate.Cfg.XmlHbmBinding
{
	internal class AuxiliaryDatabaseObjectFactory
	{
		public static IAuxiliaryDatabaseObject Create(Mappings mappings, HbmDatabaseObject databaseObjectSchema)
		{
			return databaseObjectSchema.HasDefinition()
							? CreateCustomObject(mappings, databaseObjectSchema)
							: CreateSimpleObject(databaseObjectSchema);
		}

		private static IAuxiliaryDatabaseObject CreateSimpleObject(HbmDatabaseObject databaseObjectSchema)
		{
			string createText = databaseObjectSchema.FindCreateText();
			string dropText = databaseObjectSchema.FindDropText();

			IAuxiliaryDatabaseObject simpleObject = new SimpleAuxiliaryDatabaseObject(createText, dropText);

			foreach (string dialectName in databaseObjectSchema.FindDialectScopeNames())
			{
				simpleObject.AddDialectScope(dialectName);
			}

			return simpleObject;
		}

		private static IAuxiliaryDatabaseObject CreateCustomObject(Mappings mappings, HbmDatabaseObject databaseObjectSchema)
		{
			HbmDefinition definitionSchema = databaseObjectSchema.FindDefinition();
			string customTypeName = definitionSchema.@class;

			try
			{
				string className =
					TypeNameParser.Parse(customTypeName, mappings.DefaultNamespace, mappings.DefaultAssembly).ToString();
				System.Type customType = ReflectHelper.ClassForName(className);

				IAuxiliaryDatabaseObject customObject = (IAuxiliaryDatabaseObject)Activator.CreateInstance(customType);

				Dictionary<string, string> parameters = definitionSchema.FindParameters();
				customObject.SetParameterValues(parameters);
				foreach (string dialectName in databaseObjectSchema.FindDialectScopeNames())
				{
					customObject.AddDialectScope(dialectName);
				}

				return customObject;
			}
			catch (TypeLoadException exception)
			{
				throw new MappingException(string.Format("Could not locate custom database object class [{0}].", customTypeName),
																	 exception);
			}
			catch (Exception exception)
			{
				throw new MappingException(
					string.Format("Could not instantiate custom database object class [{0}].", customTypeName), exception);
			}
		}
	}
}