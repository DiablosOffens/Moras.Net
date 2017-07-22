using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq.Expressions;
using System.Diagnostics;

namespace DelphiClasses
{
    public static class Extensions
    {
        private static Func<Component, EventHandlerList> s_eventsGetter;
        private static object s_ctrlClickEventKey;
        private static object s_toolItemClickEventKey;
        private static Dictionary<IComponent, IContainer> compChildContainers = new Dictionary<IComponent, IContainer>();

        static Extensions()
        {
            var propEvents = typeof(Component).GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic);
            s_eventsGetter = (Func<Component, EventHandlerList>)Delegate.CreateDelegate(typeof(Func<Component, EventHandlerList>), propEvents.GetGetMethod(true));
            var ctrlClickEventKeyInfo = typeof(Control).GetField("EventClick", BindingFlags.Static | BindingFlags.NonPublic);
            s_ctrlClickEventKey = ctrlClickEventKeyInfo != null ? ctrlClickEventKeyInfo.GetValue(null) : null;
            var toolItemClickEventKeyInfo = typeof(ToolStripItem).GetField("EventClick", BindingFlags.Static | BindingFlags.NonPublic);
            s_toolItemClickEventKey = toolItemClickEventKeyInfo != null ? toolItemClickEventKeyInfo.GetValue(null) : null;
        }

        public static IContainer GetChildContainer(this IComponent comp)
        {
            IContainer cont;
            if (!compChildContainers.TryGetValue(comp, out cont))
            {
                if (comp is TCustomForm && comp.Site != null)
                    cont = comp.Site.Container ?? new Container();
                else
                    cont = new Container();
                compChildContainers.Add(comp, cont);
            }
            return cont;
        }

        public static IContainer GetParentContainer(this IComponent comp)
        {
            if (!(comp is TCustomForm) && comp.Site != null)
                return comp.Site.Container;
            if (comp is Control)
            {
                TCustomForm form = ((Control)comp).FindForm() as TCustomForm;
                if (form != null)
                    return form.Site.Container;
            }
            return null;
        }

        public static object GetSource(this DragEventArgs e)
        {
            DragSource src = (DragSource)e.Data.GetData(typeof(DragSource));
            return src == null ? null : src.Source;
        }

        public static T Clamp<T>(this T value, T min, T max) where T : struct,IComparable<T>
        {
            return (value.CompareTo(min) < 0) ? min : ((value.CompareTo(max) > 0) ? max : value);
        }

        public static int ToIntDef(this string value, int defaultValue)
        {
            int result;
            if (int.TryParse(value, out result))
                return result;
            return defaultValue;
        }

        public static unsafe IntPtr ToIntPtr(this UIntPtr ptr)
        {
            return new IntPtr(ptr.ToPointer());
        }

        public static unsafe UIntPtr ToUIntPtr(this IntPtr ptr)
        {
            return new UIntPtr(ptr.ToPointer());
        }

        public static Rectangle DeflateRect(Rectangle rect, Padding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Top;
            rect.Width -= padding.Horizontal;
            rect.Height -= padding.Vertical;
            return rect;
        }

        //HINT: http://stackoverflow.com/questions/2974519/generic-constraints-where-t-struct-and-where-t-class
        public class RequireIEquatable<T> where T : IEquatable<T> { }
        public class RequireClass<T> where T : class { }
        public class RequireStruct<T> where T : struct { }
        public struct RequireIntEnumsAndBitSet<TEnum, TFlag>
            where TEnum : struct,IConvertible
            where TFlag : struct,IConvertible
        {
            public static readonly RequireIntEnumsAndBitSet<TEnum, TFlag> Default = CreateDefault();
            public bool IsValid;
            public BitSetAttribute BitSet;
            private static RequireIntEnumsAndBitSet<TEnum, TFlag> CreateDefault()
            {
                RequireIntEnumsAndBitSet<TEnum, TFlag> result = new RequireIntEnumsAndBitSet<TEnum, TFlag>();
                result.BitSet = (BitSetAttribute)typeof(TFlag).GetCustomAttributes(typeof(BitSetAttribute), false).SingleOrDefault();
                result.IsValid = typeof(TEnum).IsEnum && //Marshal.SizeOf(Enum.GetUnderlyingType(typeof(TEnum))) <= sizeof(int) && // check needed?
                    result.BitSet != null && result.BitSet.SetEnumType == typeof(TEnum);
                return result;
            }
        }

        public static bool InSet<T>(this T value, params T[] set)
        {
            IEquatable<T> eqvalue = value as IEquatable<T>;
            if (eqvalue != null)
            {
                for (int i = 0; i < set.Length; i++)
                {
                    if (eqvalue.Equals(set[i]))
                        return true;
                }
            }
            else if (value != null)
            {
                for (int i = 0; i < set.Length; i++)
                {
                    if (value.Equals(set[i]))
                        return true;
                }
            }
            return false;
        }

        public static bool InSet<TEnum, TFlag>(this TEnum bit, TFlag set, RequireIntEnumsAndBitSet<TEnum, TFlag> tag = default(RequireIntEnumsAndBitSet<TEnum, TFlag>))
            where TEnum : struct,IConvertible, IComparable
            where TFlag : struct,IConvertible
        {
            if (!RequireIntEnumsAndBitSet<TEnum, TFlag>.Default.IsValid)
                throw new NotSupportedException();
            if (!RequireIntEnumsAndBitSet<TEnum, TFlag>.Default.BitSet.IsInrange(bit))
                throw new ArgumentOutOfRangeException("bit");
            ulong bitvalue = 1UL << bit.ToInt32(null);
            ulong flagsset = set.ToUInt64(null);
            return (flagsset & bitvalue) != 0UL;
        }

        public static bool InSet<T>(this T value, BitArray set) where T : struct,IConvertible
        {
            int index = value.ToInt32(null);
            return set[index];
        }

        public static bool InSet<T>(this T value, HashSet<T> set)
        {
            return set.Contains(value);
        }

        public static string DebugFormat<T>(this HashSet<T> set)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            bool first = true;
            foreach (T item in set)
            {
                if (first)
                {
                    builder.Append(',');
                    first = false;
                }
                builder.Append(item);
            }
            builder.Append(']');

            return builder.ToString();
        }

        public static int High<T>(this T[] array)
        {
            return array.GetUpperBound(0);
        }

        public static int Low<T>(this T[] array)
        {
            return array.GetLowerBound(0);
        }

        // use is operator for constant types
        public static bool InheritsFrom(this object self, Type type)
        {
            Type selftype = (self as Type) ?? self.GetType();
            return selftype == type || selftype.IsSubclassOf(type);
        }

        // use is operator for constant types
        public static bool ImplementsInterface(this object self, Type ifaceType)
        {
            Type selftype = (self as Type) ?? self.GetType();
            for (Type type = selftype; type != null; type = type.BaseType)
            {
                Type[] interfaces = type.GetInterfaces();
                if (interfaces != null)
                {
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if ((interfaces[i] == ifaceType) || ((interfaces[i] != null) && interfaces[i].ImplementsInterface(ifaceType)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool AreIntersectingOrBothEmpty(this MulticastDelegate left, MulticastDelegate right)
        {
            if (left == null)
                return right == null;

            return right != null && left.GetInvocationList().Intersect(right.GetInvocationList()).Any();
        }

        public static void Clear(this ComboBox cmb)
        {
            //TODO: find out if we need to reset the SelectedIndex
            //cmb.SelectedIndex = -1;
            cmb.Items.Clear();
        }

        public static void ClearSelection(this ComboBox cmb)
        {
            cmb.SelectedIndex = -1;
        }

        //TODO: try to avoid this
        public static void SetSelectedIndexSafe(this ComboBox cmb, int index)
        {
            if (index < 0 || index >= cmb.Items.Count)
                cmb.SelectedIndex = -1; // behave like delphi but without crashing the app
            cmb.SelectedIndex = index;
        }

        //public static bool CanPersist(this object obj)
        //{
        //    if (obj is ISerializable)
        //        return true;
        //    if (obj.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Length != 0)
        //        return true;
        //    return false;
        //}

        public static string GetHint(this Control ctl)
        {
            Control parent = ctl;
            while (parent != null && !(parent is TCustomForm))
                parent = parent.Parent;
            if (parent == null)
                return null;

            ComponentCollection comps = GetComponents(parent);
            if (comps == null)
                return null;

            foreach (IComponent comp in comps)
            {
                ToolTip tip = comp as ToolTip;
                string hint = null;
                if (tip != null)
                    hint = tip.GetToolTip(ctl);
                if (!string.IsNullOrEmpty(hint))
                    return hint;
            }
            return null;
        }

        public static void SetHint(this Control ctl, string value)
        {
            Control parent = ctl;
            while (parent != null && !(parent is TCustomForm))
                parent = parent.Parent;
            if (parent == null)
                return;

            ComponentCollection comps = GetComponents(parent);
            if (comps == null)
                return;

            foreach (IComponent comp in comps)
            {
                ToolTip tip = comp as ToolTip;
                if (tip != null)
                {
                    tip.SetToolTip(ctl, value);
                    //break; // TODO: only first?
                }
            }
        }

        public static void SetHelpKeyword(this Control ctl, string value)
        {
            Control parent = ctl;
            while (parent != null && !(parent is TCustomForm))
                parent = parent.Parent;
            if (parent == null)
                return;

            ComponentCollection comps = GetComponents(parent);
            if (comps == null)
                return;

            foreach (IComponent comp in comps)
            {
                HelpProvider help = comp as HelpProvider;
                if (help != null)
                {
                    help.SetHelpKeyword(ctl, value);
                    //break; // TODO: only first?
                }
            }
        }

        public static string GetName(this IComponent comp)
        {
            if (comp is Control)
                return ((Control)comp).Name;
            if (comp.Site != null)
                return comp.Site.Name;
            return null;
        }

        public static ComponentCollection GetComponents(this IComponent comp)
        {
            if (comp == null)
                throw new ArgumentNullException("comp");

            return GetChildContainer(comp).Components;
        }

        public static IComponent FindComponent(this IComponent comp, string name)
        {
            if (comp == null)
                throw new ArgumentNullException("comp");

            ComponentCollection comps = GetComponents(comp);
            return comps[name];
        }

        public static void InsertComponent(this IComponent compOwner, IComponent compToAdd, string name)
        {
            if (compOwner == null)
                throw new ArgumentNullException("compOwner");
            if (compToAdd == null)
                throw new ArgumentNullException("compToAdd");

            GetChildContainer(compOwner).Add(compToAdd, name);
            if (compOwner is Control && compToAdd is Control)
            {
                ((Control)compToAdd).Name = name;
                ((Control)compOwner).Controls.Add((Control)compToAdd);
            }
        }

        public static EventHandler GetClickEvent(this Control ctrl)
        {
            if (s_ctrlClickEventKey == null)
                return null;
            var handler = s_eventsGetter(ctrl);
            return (EventHandler)handler[s_ctrlClickEventKey];
        }

        public static EventHandler GetClickEvent(this ToolStripItem ctrl)
        {
            if (s_toolItemClickEventKey == null)
                return null;
            var handler = s_eventsGetter(ctrl);
            return (EventHandler)handler[s_toolItemClickEventKey];
        }

        //TODO: needs much reflection hacks, if EventHandlerList would implement IEnumerable, this would be much easier
        //public static void RemoveAllHandlersOfObject(this Control ctl, object target)
        //{
        //    EventHandlerList events = s_eventsGetter(ctl);
        //    foreach (Delegate handler in events)
        //    {

        //    }
        //}

        public static void SetShadowProperty<T>(this Control ctl, Expression<Func<T>> expProp, T value)
        {
            if (ctl == null)
                throw new ArgumentNullException("ctl");

            MemberExpression exp = expProp.Body as MemberExpression;
            if (exp == null || exp.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException("Expression must be a property access expression.", "expProp");

            ISite site = ctl.Site;
            if (site != null && site.DesignMode)
            {
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(ctl);
                PropertyDescriptor designerProp = props[exp.Member.Name];
                if (designerProp == null)
                    throw new ArgumentException("The property must be a member of the control object.", "expProp");
                designerProp.SetValue(ctl, value);
            }
            else
            {
                PropertyInfo rawProp = (PropertyInfo)exp.Member;
                rawProp.SetValue(ctl, value, null);
            }
        }

        public static void SelectNextPage(this TabControl ctl, bool goForward)
        {
            int selectedIndex = ctl.SelectedIndex;
            if (selectedIndex != -1)
            {
                int tabCount = ctl.TabCount;
                if (goForward)
                {
                    ctl.SelectedIndex = (selectedIndex + 1) % tabCount;
                }
                else
                {
                    ctl.SelectedIndex = ((selectedIndex + tabCount) - 1) % tabCount;
                }
            }
        }

        public static bool ExecuteAction(this Component self, DelphiClasses.TBasicAction action)
        {
            //simulate inheritance
            if (self is DelphiClasses.TActionList)
                return ((DelphiClasses.TActionList)self).ExecuteAction(action);

            if (action.HandlesTarget(self))
            {
                action.ExecuteTarget(self);
                return true;
            }
            else
                return false;
        }

        public static bool UpdateAction(this Component self, DelphiClasses.TBasicAction action)
        {
            //simulate inheritance
            if (self is DelphiClasses.TActionList)
                return ((DelphiClasses.TActionList)self).UpdateAction(action);

            if (action.HandlesTarget(self))
            {
                action.UpdateTarget(self);
                return true;
            }
            else
                return false;
        }

        internal class IdentityFunction<TElement>
        {
            public static Func<TElement, TElement> Instance
            {
                get { return x => x; }
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(IdentityFunction<IEnumerable<T>>.Instance);
        }

        public static UnmanagedMemoryStream ToStream(this SafeBuffer buffer, FileAccess access = FileAccess.Read)
        {
            return new UnmanagedMemoryStream(buffer, 0, (long)buffer.ByteLength, access);
        }

        public static void SaveToFile(this MemoryStream stream, string path)
        {
            using (Stream outstream = File.Create(path))
            {
                stream.WriteTo(outstream);
            }
        }

        public static void SaveToFile(this string[] lines, string path)
        {
            using (Stream outstream = File.Create(path))
            {
                if (lines.Length == 0) return;
                using (StreamWriter writer = new StreamWriter(outstream, Encoding.Default))
                {
                    writer.Write(lines[0]);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        writer.WriteLine();
                        writer.Write(lines[i]);
                    }
                }
            }
        }

        public static string[] LoadFromFile(string path)
        {
            return File.ReadAllLines(path, Encoding.Default);
        }

        public static Process ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, ProcessWindowStyle nShowCmd)
        {
            ProcessStartInfo shellexec = new ProcessStartInfo(lpFile, lpParameters);
            shellexec.UseShellExecute = true;
            shellexec.ErrorDialog = true;
            shellexec.ErrorDialogParentHandle = hwnd;
            shellexec.Verb = lpOperation;
            shellexec.WindowStyle = nShowCmd;
            shellexec.WorkingDirectory = lpDirectory;
            return Process.Start(shellexec);
        }
    }
}
