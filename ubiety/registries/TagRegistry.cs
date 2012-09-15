// TagRegistry.cs
//
//Ubiety XMPP Library Copyright (C) 2006 - 2012 Dieter Lunn
// 
// This library is free software; you can redistribute it and/or modify it under
// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
// 
// This library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
// 
// You should have received a copy of the GNU Lesser General Public License along
// with this library; if not, write to the Free Software Foundation, Inc., 59
// Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.Reflection;
using System.Xml;
using ubiety.common;
using ubiety.common.attributes;
using ubiety.common.logging;
using ubiety.states;

namespace ubiety.registries
{
	/// <remarks>
	/// TagRegistry stores all the construction information for the <seealso cref="Tag">Tags</seealso> the library is aware of.
	/// </remarks>
	public sealed class TagRegistry : Registry<TagRegistry, RegistryAllocator<TagRegistry>>
	{
		private TagRegistry()
		{
		}

		/// <summary>
		/// Used to add <seealso cref="Tag">Tag(s)</seealso> to the registry.  Using attributes the <see cref="TagRegistry"/> looks for and adds any appropriate tags found in the assembly.
		/// </summary>
		/// <param name="ass">The assembly to search for tags</param>
		public void AddAssembly(Assembly assembly)
		{
			Logger.DebugFormat(this, "Adding assembly {0}", assembly.FullName);

			var tags = GetAttributes<XmppTagAttribute>(assembly);
			Logger.DebugFormat(this, "{0,-24}{1,-36}{2}", "Tag Name", "Class", "Namespace");
			foreach (var tag in tags)
			{
				Logger.DebugFormat(this, "{0,-24}{1,-36}{2}", tag.Name, tag.ClassType.FullName, tag.Ns);
				RegisteredItems.Add(new XmlQualifiedName(tag.Name, tag.Ns).ToString(), tag.ClassType);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="ns"></param>
		/// <param name="doc"></param>
		/// <returns></returns>
		public Tag GetTag(string name, string ns)
		{
            return GetTag(new XmlQualifiedName(name, ns));
		}

		/// <summary>
		/// Creates a new instance of the wanted tag.
		/// </summary>
		/// <param name="qname">Qualified Namespace</param>
		/// <param name="doc">XmlDocument to create tag with</param>
		/// <returns>A new instance of the requested tag</returns>
		public Tag GetTag(XmlQualifiedName qname)
		{
			Tag tag = null;

			Logger.DebugFormat(this, "Finding tag: {0}", qname);

			Type t;
			if (RegisteredItems.TryGetValue(qname.ToString(), out t))
			{
				//tag = (Tag)Activator.CreateInstance(t, new object[] { doc });
				var ctor = t.GetConstructor(new[] {typeof (XmlDocument)});
				if (ctor == null)
				{
					ctor = t.GetConstructor(new[] {typeof (XmlDocument), typeof (XmlQualifiedName)});
					if (ctor != null) tag = (Tag) ctor.Invoke(new object[] {ProtocolState.Document, qname});
				}
				else
				{
					tag = (Tag) ctor.Invoke(new object[] {ProtocolState.Document});
				}
			}
			else
			{
				Errors.SendError(this, ErrorType.UnregisteredItem,
				                          "Tag " + qname + " not found in registry.  Please load appropriate library.");
				return null;
			}

            return tag;
		}
	}
}