using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace Microsoft.Austria.WcfHelpers.SoapWithAttachments
{
    public class SwaEncoderFactory : MessageEncoderFactory
    {
        protected readonly SwaEncoder _Encoder;

        public override MessageEncoder Encoder
        {
            get { return _Encoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return _Encoder.MessageVersion; }
        }

        public SwaEncoderFactory(MessageEncoderFactory encoderFactory)
        {
            if (encoderFactory == null)
            {
                throw new ArgumentNullException("encoderFactory",
                    "You need to pass an inner encoder to the SwaEncoderFactory to support SOAP-message processing!");
            }
            else
            {
                _Encoder = new SwaEncoder(encoderFactory.Encoder, this);
            }
        }

        public override MessageEncoder CreateSessionEncoder()
        {
            return base.CreateSessionEncoder();
        }
    }
}
