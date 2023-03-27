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

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace libfintx.EBICSConfig
{
    public abstract class KeyPair<T> : IDisposable
    {
        
        
        protected RSA _publicKey;
        protected RSA _privateKey;
        protected X509Certificate2 _cert;

        public X509Certificate2 Certificate
        {
            get => _cert;
            set
            {
                _cert = value;

                if (_cert == null)
                {
                    return;
                }

                _publicKey = _cert.GetRSAPublicKey();
                _privateKey = _cert.GetRSAPrivateKey();
            }
        }

        public RSA PrivateKey
        {
            get => _privateKey;
            set => _privateKey = value;
        }

        public RSA PublicKey
        {
            get => _publicKey;
            set => _publicKey = value;
        }

        public byte[] Digest
        {
            get
            {
                if (_publicKey == null)
                {
                    return null;
                }

                var p = _publicKey.ExportParameters(false);
                var hexExp = BitConverter.ToString(p.Exponent).Replace("-", string.Empty).ToLower()
                    .TrimStart('0');
                var hexMod = BitConverter.ToString(p.Modulus).Replace("-", string.Empty).ToLower()
                    .TrimStart('0');
                var hashInput = Encoding.ASCII.GetBytes(string.Format("{0} {1}", hexExp, hexMod));

                using (var sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(hashInput);
                }
            }
        }

        public DateTime? TimeStamp { get; set; }

        public T Version { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _publicKey?.Dispose();
                _privateKey?.Dispose();
                _cert?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        static KeyPair()
        {
            
        }

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
