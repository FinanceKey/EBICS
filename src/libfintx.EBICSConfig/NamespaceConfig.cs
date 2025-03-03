﻿/*	
 * 	
 *  This file is part of libfintx.
 *  
 *  Copyright (C) 2018 Bjoern Kuensting
 *  
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 3 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program; if not, write to the Free Software Foundation,
 *  Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 * 	
 */

using System.Text.Json;

namespace libfintx.EBICSConfig
{
    public class NamespaceConfig
    {


        public string Ebics { get; set; }

        public string EbicsPrefix => "urn";

        public string XmlDsig { get; set; }

        public string XmlDsigPrefix => "ds";

        public string Cct { get; set; }
        public string Cdd { get; set; }

        public string SignatureData { get; set; }
        
        static NamespaceConfig()
        {

        }

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
