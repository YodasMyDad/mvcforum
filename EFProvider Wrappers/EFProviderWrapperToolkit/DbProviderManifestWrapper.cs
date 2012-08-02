// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Data.Common;
using System.Xml;
using System.Xml.Linq;

namespace EFProviderWrapperToolkit
{
    /// <summary>
    /// Wrapper for <see cref="DbProviderManifest"/> objects.
    /// </summary>
    public class DbProviderManifestWrapper : DbProviderManifest
    {
        private string providerInvariantName;
        private DbProviderManifest wrappedProviderManifest;
        private string wrapperProviderInvariantName;

        /// <summary>
        /// Initializes a new instance of the DbProviderManifestWrapper class.
        /// </summary>
        /// <param name="wrapperProviderInvariantName">Wrapper provider invariant name.</param>
        /// <param name="providerInvariantName">Provider invariant name.</param>
        /// <param name="wrappedProviderManifest">The wrapped provider manifest.</param>
        public DbProviderManifestWrapper(string wrapperProviderInvariantName, string providerInvariantName, DbProviderManifest wrappedProviderManifest)
        {
            this.wrapperProviderInvariantName = wrapperProviderInvariantName;
            this.providerInvariantName = providerInvariantName;
            this.wrappedProviderManifest = wrappedProviderManifest;
        }

        /// <summary>
        /// Gets the namespace name supported by this provider manifest.
        /// </summary>
        /// <value></value>
        /// <returns>The namespace name supported by this provider manifest.</returns>
        public override string NamespaceName
        {
            get { return this.wrappedProviderManifest.NamespaceName; }
        }

        /// <summary>
        /// Gets the wrapped provider manifest.
        /// </summary>
        /// <value>The wrapped provider manifest.</value>
        public DbProviderManifest WrappedProviderManifest
        {
            get { return this.wrappedProviderManifest; }
        }

        /// <summary>
        /// Gets the invariant name of the wrapped provider.
        /// </summary>
        /// <value>The invariant name of the wrapped provider.</value>
        public string WrappedProviderManifestInvariantName
        {
            get { return this.providerInvariantName; }
        }

        /// <summary>
        /// Returns the best mapped equivalent EDM type for a specified storage type.
        /// </summary>
        /// <param name="storeType">The instance of the <see cref="T:System.Data.Metadata.Edm.TypeUsage"/> class that encapsulates both a storage type and a set of facets for that type.</param>
        /// <returns>
        /// The instance of that <see cref="T:System.Data.Metadata.Edm.TypeUsage"/> encapsulates both an EDM type and a set of facets for that type.
        /// </returns>
        public override System.Data.Metadata.Edm.TypeUsage GetEdmType(System.Data.Metadata.Edm.TypeUsage storeType)
        {
            return this.wrappedProviderManifest.GetEdmType(storeType);
        }

        /// <summary>
        /// Returns the list of facet descriptions for the specified Entity Data Model (EDM) type.
        /// </summary>
        /// <param name="edmType">An <see cref="T:System:Data.Metadata.Edm.EdmType"/> for which the facet descriptions are to be retrieved.</param>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> that contains the list of facet descriptions for the specified Entity Data Model (EDM) type.
        /// </returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<System.Data.Metadata.Edm.FacetDescription> GetFacetDescriptions(System.Data.Metadata.Edm.EdmType edmType)
        {
            return this.wrappedProviderManifest.GetFacetDescriptions(edmType);
        }

        /// <summary>
        /// Returns the list of provider-specific functions.
        /// </summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> that contains the list of provider-specific functions.
        /// </returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<System.Data.Metadata.Edm.EdmFunction> GetStoreFunctions()
        {
            return this.wrappedProviderManifest.GetStoreFunctions();
        }

        /// <summary>
        /// Returns the best mapped equivalent storage type for a specified Entity Data Model (EDM) type.
        /// </summary>
        /// <param name="edmType">The instance of the <see cref="T:System.Data.Metadata.Edm.TypeUsage"/> class that encapsulates both an EDM type and a set of facets for that type.</param>
        /// <returns>
        /// The instance of the <see cref="T:System.Data.Metadata.Edm.TypeUsage"/> class that encapsulates both a storage type and a set of facets for that type.
        /// </returns>
        public override System.Data.Metadata.Edm.TypeUsage GetStoreType(System.Data.Metadata.Edm.TypeUsage edmType)
        {
            return this.wrappedProviderManifest.GetStoreType(edmType);
        }

        /// <summary>
        /// Returns the list of primitive types that are supported by the storage provider.
        /// </summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> that contains the list of primitive types supported by the storage provider.
        /// </returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<System.Data.Metadata.Edm.PrimitiveType> GetStoreTypes()
        {
            return this.wrappedProviderManifest.GetStoreTypes();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "DbProviderManifestWrapper(Wrapped=" + this.wrappedProviderManifest + ")";
        }

        /// <summary>
        /// Indicates if the provider supports escaping strings to be used as patterns in a Like expression.
        /// </summary>
        /// <param name="escapeCharacter">If the provider supports escaping, the character that would be used as the escape character.</param>
        /// <returns>
        /// Returns true if this provider supports escaping strings to be used as patterns in a Like expression; otherwise, false. The default implementation returns false.
        /// </returns>
        public override bool SupportsEscapingLikeArgument(out char escapeCharacter)
        {
            return this.wrappedProviderManifest.SupportsEscapingLikeArgument(out escapeCharacter);
        }

        /// <summary>
        /// Provider writers should override this method to return the argument with the wildcards and the escape  character escaped. This method is only used if <see cref="M:System.Data.Common.DbProviderManifest.SupportsEscapingLikeArgument(System.Char@)"/> returns true.
        /// </summary>
        /// <param name="argument">The argument to be escaped.</param>
        /// <returns>
        /// The argument with the wildcards and the escape character escaped.
        /// </returns>
        public override string EscapeLikeArgument(string argument)
        {
            return this.wrappedProviderManifest.EscapeLikeArgument(argument);
        }

        /// <summary>
        /// Returns an XML reader that represents the storage provider-specific information.
        /// </summary>
        /// <param name="informationType">The name of the information to be retrieved. Providers are required to support the following values: <see cref="F:System.Data.Common.DbProviderManifest.StoreSchemaDefinition"/> or <see cref="F:System.Data.Common.DbProviderManifest.StoreSchemaMapping"/>.</param>
        /// <returns>
        /// An <see cref="T:System.Xml.XMLReader"/> object that represents the storage provider-specific information.
        /// </returns>
        protected override System.Xml.XmlReader GetDbInformation(string informationType)
        {
            if (informationType == DbProviderManifest.StoreSchemaDefinition)
            {
                return this.InjectProviderNameIntoSsdl(this.wrappedProviderManifest.GetInformation(informationType));
            }

            return this.wrappedProviderManifest.GetInformation(informationType);
        }

        /// <summary>
        /// Injects the provider name into SSDL.
        /// </summary>
        /// <param name="originalReader">The original reader for the XML stream representing SSDL.</param>
        /// <returns>XmlReader for the modified SSDL</returns>
        private XmlReader InjectProviderNameIntoSsdl(XmlReader originalReader)
        {
            XElement ssdl = XElement.Load(originalReader);
            ssdl.Attribute("ProviderManifestToken").Value = ssdl.Attribute("Provider").Value + ";" + ssdl.Attribute("ProviderManifestToken").Value;
            ssdl.Attribute("Provider").Value = this.wrapperProviderInvariantName;
            return ssdl.CreateReader();
        }
    }
}
