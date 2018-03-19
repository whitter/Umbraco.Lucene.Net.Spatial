using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Tier;
using Lucene.Net.Spatial.Tier.Projectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UmbracoLuceneNetSpatial
{
    public static class SearchService
    {
        public static ScoreDoc[] Search(out int count, double lat, double lng, double radius)
        {
            LuceneIndexer indexer = (LuceneIndexer)ExamineManager.Instance.IndexProviderCollection["ExternalIndexer"];
            IndexSearcher searcher = new IndexSearcher(indexer.GetLuceneDirectory(), false);
            BooleanQuery criteria = GetBaseCriteria();

            Filter distanceFilter = new DistanceQueryBuilder(lat, lng, radius, "_lat", "_long", CartesianTierPlotter.DefaltFieldPrefix, true).DistanceFilter;

            //***AT OTHER SEARCH CRITERIA***

            var results = searcher.Search(criteria, distanceFilter, searcher.MaxDoc());

            count = results.TotalHits;

            return results.ScoreDocs;
        }

        private static BooleanQuery GetBaseCriteria()
        {
            var criteria = new BooleanQuery();

            criteria.Add(new TermQuery(new Term("nodeTypeAlias", "locationitem")), BooleanClause.Occur.MUST);            

            return criteria;
        }

    }
}