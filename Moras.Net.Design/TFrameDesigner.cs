using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design.Serialization;

namespace Moras.Net.Design
{
    //HINT: this don't work perfectly, so use it only if you actually need it
    public class TFrameDesigner : ControlDesigner
    {
        private class InlineInheritanceService : IInheritanceService, IDisposable
        {
            internal readonly IInheritanceService m_parentService;
            private readonly TFrameDesigner m_owner;
            private IDesignerSerializationManager m_manager;
            private HashSet<IComponent> m_inheritedComponents = new HashSet<IComponent>();

            public InlineInheritanceService(TFrameDesigner owner, IInheritanceService parentService)
            {
                m_parentService = parentService;
                m_owner = owner;
            }

            ~InlineInheritanceService()
            {
                Dispose(false);
            }

            private static string GetFullTypeName<T>()
            {
                return typeof(T).FullName;
            }

            private TypeDescriptionProvider GetTargetFrameworkProviderForType(Type type)
            {
                if (m_manager == null)
                    return null;

                TypeDescriptionProviderService service = m_manager.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
                if (service == null)
                    return null;

                return service.GetProvider(type);
            }

            private Type GetReflectionTypeFromTypeHelper(Type type)
            {
                if (type != null)
                {
                    TypeDescriptionProvider targetFrameworkProviderForType = GetTargetFrameworkProviderForType(type);
                    if ((targetFrameworkProviderForType != null) && targetFrameworkProviderForType.IsSupportedType(type))
                    {
                        return targetFrameworkProviderForType.GetReflectionType(type);
                    }
                }
                return type;
            }

            private bool IgnoreInheritedMember(MemberInfo member, IComponent component)
            {
                if (member is FieldInfo)
                {
                    FieldInfo info = (FieldInfo)member;
                    return info.IsPrivate || info.IsAssembly;
                }
                else if (member is MethodInfo)
                {
                    MethodInfo info = (MethodInfo)member;
                    return info.IsPrivate || info.IsAssembly;
                }
                return true;
            }

            #region IInheritanceService Members

            public void AddInheritedComponents(IComponent component, IContainer container)
            {
                if (component != m_owner.Component || !m_owner.Inline)
                {
                    if (m_parentService != null)
                        m_parentService.AddInheritedComponents(component, container);
                    return;
                }

                ISite site = component.Site;
                INameCreationService nameService = null;
                if (site != null)
                    nameService = (INameCreationService)site.GetService(typeof(INameCreationService));

                IDesignerHost host = (IDesignerHost)m_owner.GetService(typeof(IDesignerHost));
                if (host != null)
                    m_manager = (IDesignerSerializationManager)host.GetService(typeof(IDesignerSerializationManager));

                try
                {
                    Type compType = component.GetType();
                    string stopType = GetFullTypeName<DelphiClasses.TFrame>();
                    while (compType != typeof(object) && compType.FullName != stopType)
                    {
                        Type reflectionType = TypeDescriptor.GetReflectionType(compType);
                        foreach (FieldInfo info in reflectionType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        {
                            string name = info.Name;
                            Type reflectionTypeFromTypeHelper = GetReflectionTypeFromTypeHelper(info.FieldType);
                            if (GetReflectionTypeFromTypeHelper(typeof(IComponent)).IsAssignableFrom(reflectionTypeFromTypeHelper))
                            {
                                object obj2 = info.GetValue(component);
                                if (obj2 != null)
                                {
                                    MemberInfo member = info;
                                    object[] customAttributes = info.GetCustomAttributes(typeof(AccessedThroughPropertyAttribute), false);
                                    if ((customAttributes != null) && (customAttributes.Length != 0x0))
                                    {
                                        AccessedThroughPropertyAttribute attribute2 = (AccessedThroughPropertyAttribute)customAttributes[0x0];
                                        PropertyInfo property = reflectionType.GetProperty(attribute2.PropertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                        if ((property != null) && (property.PropertyType == info.FieldType))
                                        {
                                            if (!property.CanRead)
                                                continue;

                                            member = property.GetGetMethod(true);
                                            name = attribute2.PropertyName;
                                        }
                                    }

                                    if (!IgnoreInheritedMember(member, component) &&
                                        m_inheritedComponents.Add((IComponent)obj2))
                                    {
                                        if (nameService == null || nameService.IsValidName(name))
                                        {
                                            try
                                            {
                                                container.Add((IComponent)obj2, name);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        compType = compType.BaseType;
                    }
                }
                finally
                {
                    m_manager = null;
                }
            }

            public InheritanceAttribute GetInheritanceAttribute(IComponent component)
            {
                if (m_inheritedComponents.Contains(component))
                    return InheritanceAttribute.Inherited;
                if (m_parentService != null)
                    return m_parentService.GetInheritanceAttribute(component);
                return InheritanceAttribute.Default;
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            protected virtual void Dispose(bool disposing)
            {
                if (disposing && (m_inheritedComponents != null))
                {
                    m_inheritedComponents.Clear();
                    m_inheritedComponents = null;
                }
            }

            public void ClearInlines(IContainer container)
            {
                foreach (var comp in m_inheritedComponents)
                {
                    container.Remove(comp);
                }
                m_inheritedComponents.Clear();
            }
        }

        private bool m_inline;
        private InlineInheritanceService m_inheritanceService;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    if (m_inheritanceService != null)
                    {
                        host.RemoveService(typeof(IInheritanceService));
                        if (m_inheritanceService.m_parentService != null)
                            host.AddService(typeof(IInheritanceService), m_inheritanceService.m_parentService);
                        m_inheritanceService.Dispose();
                        m_inheritanceService = null;
                    }
                }
            }
            base.Dispose(disposing);
        }

        public override void Initialize(System.ComponentModel.IComponent component)
        {
            base.Initialize(component);

            IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (host == null)
                return;

            IInheritanceService inheritanceService = (IInheritanceService)GetService(typeof(IInheritanceService));
            m_inheritanceService = new InlineInheritanceService(this, inheritanceService);
            host.RemoveService(typeof(IInheritanceService));
            host.AddService(typeof(IInheritanceService), m_inheritanceService);
        }

        protected override void PreFilterProperties(System.Collections.IDictionary properties)
        {
            base.PreFilterProperties(properties);

            Attribute[] inlineAttribs = new Attribute[] { new DefaultValueAttribute(false), BrowsableAttribute.Yes, CategoryAttribute.Design, DesignOnlyAttribute.Yes/*, new DescriptionAttribute("Inline")*/ };
            properties["Inline"] = TypeDescriptor.CreateProperty(typeof(TFrameDesigner), "Inline", typeof(bool), inlineAttribs);
        }

        private bool Inline
        {
            get { return m_inline; }
            set
            {
                if (m_inline != value)
                {
                    m_inline = value;

                    INestedContainer nestedContainer = this.GetService(typeof(INestedContainer)) as INestedContainer;
                    if (nestedContainer == null)
                        return;

                    if (value)
                    {
                        m_inheritanceService.AddInheritedComponents(Component, nestedContainer);
                    }
                    else
                    {
                        m_inheritanceService.ClearInlines(nestedContainer);
                    }
                }
            }
        }
    }
}
