using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MasterResolver9000
{
    public class DNSParser
    {
        public class DnsLite
        {
            private byte[] _data;
            private int _position, _id, _length;
            private string _name;

            private static int DNS_PORT = 53;
            private Encoding _ascii = Encoding.ASCII;

            public DnsLite()
            {
                _id = DateTime.Now.Millisecond * 60;
                
            }

            private int GetNewId()
            {
                return ++_id;
            }

            public async Task<List<MXRecord>> GetMXRecordsAsync(List<string> hosts, string dns)
            {
                List<MXRecord> mxRecords = new List<MXRecord>();

                using (UdpClient dnsClient = new UdpClient(dns, DNS_PORT))
                {
                    dnsClient.Client.ReceiveTimeout = 5000;

                    foreach (string host in hosts)
                    {
                        MakeQuery(GetNewId(), host);

                        try
                        {
                            await dnsClient.SendAsync(_data, _data.Length);

                            UdpReceiveResult result = await dnsClient.ReceiveAsync();
                            _data = result.Buffer;
                            _length = _data.Length;

                            mxRecords.AddRange(MakeResponse(dns, host));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }

                return mxRecords;
            }

            public void MakeQuery(int id, string name)
            {
                _data = new byte[512];

                for (int i = 0; i < 512; ++i)
                {
                    _data[i] = 0;
                }

                _data[0] = (byte)(id >> 8);
                _data[1] = (byte)(id & 0xFF);
                _data[2] = 1;
                _data[3] = 0;
                _data[4] = 0;
                _data[5] = 1;
                _data[6] = 0;
                _data[7] = 0;
                _data[8] = 0;
                _data[9] = 0;
                _data[10] = 0;
                _data[11] = 0;

                string[] tokens = name.Split('.');
                _position = 12;

                foreach (string token in tokens)
                {
                    _data[_position++] = (byte)(token.Length & 0xFF);
                    byte[] bytes = _ascii.GetBytes(token);

                    foreach (byte b in bytes)
                    {
                        _data[_position++] = b;
                    }
                }

                _data[_position++] = 0;
                _data[_position++] = 0;
                _data[_position++] = 15;
                _data[_position++] = 0;
                _data[_position++] = 1;
            }

            public List<MXRecord> MakeResponse(string dnsServerAddress, string host)
            {
                List<MXRecord> mxRecords = new List<MXRecord>();
                MXRecord mxRecord;

                int qCount = ((_data[4] & 0xFF) << 8) | (_data[5] & 0xFF);
                if (qCount < 0)
                {
                    throw new IOException("invalid question count");
                }

                int aCount = ((_data[6] & 0xFF) << 8) | (_data[7] & 0xFF);
                if (aCount < 0)
                {
                    throw new IOException("invalid answer count");
                }

                _position = 12;

                for (int i = 0; i < qCount; ++i)
                {
                    _name = string.Empty;
                    _position = ProcessPosition(_position);
                    _position += 4;
                }

                for (int i = 0; i < aCount; ++i)
                {
                    _name = string.Empty;
                    _position = ProcessPosition(_position);

                    _position += 10;

                    int pref = (_data[_position++] << 8) | (_data[_position++] & 0xFF);

                    _name = string.Empty;
                    _position = ProcessPosition(_position);

                    mxRecord = new MXRecord();

                    mxRecord.InputMail = host;
                    mxRecord.Preference = pref;
                    mxRecord.MailServer = _name;
                    mxRecord.DnsServer = dnsServerAddress;

                    if (mxRecord.MailServer == "localhost")
                    {
                        continue;
                    }

                    mxRecords.Add(mxRecord);
                }

                return mxRecords;
            }

            private int ProcessPosition(int position)
            {
                int len = (_data[position++] & 0xFF);

                if (len == 0)
                {
                    return position;
                }

                int offset;

                do
                {
                    if ((len & 0xC0) == 0xC0)
                    {
                        if (position >= _length)
                        {
                            return -1;
                        }

                        offset = ((len & 0x3F) << 8) | (_data[position++] & 0xFF);
                        ProcessPosition(offset);
                        return position;
                    }
                    else
                    {
                        if ((position + len) > _length)
                        {
                            return -1;
                        }

                        _name += _ascii.GetString(_data, position, len);
                        position += len;
                    }

                    if (position > _length)
                    {
                        return -1;
                    }

                    len = _data[position++] & 0xFF;

                    if (len != 0)
                    {
                        _name += ".";
                    }
                } while (len != 0);

                return position;
            }
        }
    }
}