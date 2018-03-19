using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Spatial.Tier.Projectors;
using Lucene.Net.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;

namespace UmbracoLuceneNetSpatial
{
    public class UmbracoExamineSpatialConfig : IApplicationEventHandler
    {
        private readonly List<CartesianTierPlotter> _ctps = new List<CartesianTierPlotter>();
        private readonly IProjector _projector = new SinusoidalProjector();

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            CartesianTierPlotter ctp = new CartesianTierPlotter(0, _projector, CartesianTierPlotter.DefaltFieldPrefix);
            int startTier = ctp.BestFit(25);
            int endTier = ctp.BestFit(5);

            for (int i = startTier; i <= endTier; i++)
            {
                _ctps.Add(new CartesianTierPlotter(i, _projector, CartesianTierPlotter.DefaltFieldPrefix));
            }

            if (applicationContext.IsConfigured && applicationContext.DatabaseContext.IsDatabaseConfigured)
            {
                var indexer = (LuceneIndexer)ExamineManager.Instance.IndexProviderCollection["ExternalIndexer"];
                indexer.DocumentWriting += new EventHandler<DocumentWritingEventArgs>(Indexer_DocumentWriting);
            }

        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        private void Indexer_DocumentWriting(object sender, DocumentWritingEventArgs e)
        {
            if (e.Fields["nodeTypeAlias"] == "locationItem")
            {
                var location = new Terratype.Models.Model(e.Fields["location"]);

                e.Document.Add(new Field("_lat", NumericUtils.DoubleToPrefixCoded(location.Position.ToWgs84().Latitude), Field.Store.YES, Field.Index.NOT_ANALYZED));
                e.Document.Add(new Field("_long", NumericUtils.DoubleToPrefixCoded(location.Position.ToWgs84().Longitude), Field.Store.YES, Field.Index.NOT_ANALYZED));

                foreach(CartesianTierPlotter ctp in _ctps)
                {
                    double boxId = ctp.GetTierBoxId(location.Position.ToWgs84().Latitude, location.Position.ToWgs84().Longitude);

                    e.Document.Add(new Field(ctp.GetTierFieldName(), NumericUtils.DoubleToPrefixCoded(boxId), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
                }
            }

        }        
    }
}