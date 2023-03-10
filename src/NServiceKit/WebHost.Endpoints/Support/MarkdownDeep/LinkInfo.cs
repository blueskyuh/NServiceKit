// 
//   MarkdownDeep - http://www.toptensoftware.com/markdowndeep
//	 Copyright (C) 2010-2011 Topten Software
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this product except in 
//   compliance with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License is 
//   distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkdownDeep
{
	internal class LinkInfo
	{
        /// <summary>Initializes a new instance of the MarkdownDeep.LinkInfo class.</summary>
        ///
        /// <param name="def">      The definition.</param>
        /// <param name="link_text">The link text.</param>
		public LinkInfo(LinkDefinition def, string link_text)
		{
			this.def = def;
			this.link_text = link_text;
		}

        /// <summary>The definition.</summary>
		public LinkDefinition def;
        /// <summary>The link text.</summary>
		public string link_text;
	}

}
