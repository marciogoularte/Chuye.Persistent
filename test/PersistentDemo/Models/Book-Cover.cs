using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace PersistentDemo.Models {
    public class Cover {
        public virtual Int32 Id { get; set; }
        public virtual String Picture { get; set; }
        public virtual Int32 BookId { get; set; }
    }

    public class Book {
        public virtual Int32 Id { get; set; }
        public virtual String Author { get; set; }
        public virtual String Title { get; set; }
        public virtual Cover Cover { get; set; }
    }

    class CoverMap : ClassMap<Cover> {
        public CoverMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Picture);
            Map(x => x.BookId);
        }
    }

    class BookMap : ClassMap<Book> {
        public BookMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            Map(x => x.Author);
            HasOne(x => x.Cover)
                .LazyLoad(Laziness.NoProxy)
                //.Constrained()
                .PropertyRef(x => x.BookId)
                .Cascade.All();
        }
    }
}
