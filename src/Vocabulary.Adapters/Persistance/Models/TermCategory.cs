using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Adapters.Persistance.Models
{
    public class TermCategory
    {
        internal TermCategory() { }

        public Guid CategoryId { get; init; }
        public Guid TermId { get; init; }
    }
}
