using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Dapper;
using Domain.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;

namespace Infrastructure.Repository
{
    public class OfferRepository : IOfferRepository
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public OfferRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public ProductsBranchOffer GetProductOffersByLocationPoint(Guid branch_id, FilterOffer filter)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var sql = $@" select
                pp.admin_id,
                bp.type,
                p.product_id,
                p.name,
                p.description,
                p.sale_price,
                p.identifier,
                p.image_default,
                pi.url,
                (SELECT row_to_json(branch)
                    FROM (
                    SELECT b.*,
                    to_jsonb(a.*) address,
                    ap.avatar,
                    coalesce((SUM(r.rating_value)/COUNT(DISTINCT(br.rating_id))),0) ratings,
			       (select count(oi.product_id) from orders.orders o
			       join orders.orders_itens oi on oi.order_id = o.order_id
			       where o.branch_id = aa.branch_id 
			       ) ordersNumbers
                    FROM partner.branch b
                    inner join partner.address a on a.branch_id = b.branch_id
                    left join reputation.branch_rating br on b.branch_id = br.branch_id
                    left join reputation.rating r on br.rating_id = r.rating_id 
                	where b.branch_id = aa.branch_id group by b.branch_id,a.*
                    ) branch
                ) AS branch,
                ( 
                    SELECT json_agg(image)
                    FROM (
                    SELECT c2.*
                    FROM catalog.product_image_product c 
                    JOIN catalog.product_image c2 ON c.product_image_id = c2.product_image_id
                    WHERE c.product_id = p.product_id 
                    ) image
                ) AS images,
                ( 
                    SELECT json_agg(category)
                    FROM (
                    SELECT c.*,(select description as category_parent_name from catalog.category where category_id = c.category_parent_id)
                    FROM catalog.category_base_product cbp 
                    JOIN catalog.category c ON c.category_id = cbp.category_id
                    WHERE cbp.product_id = p.base_product_id 
                    ) category
                ) AS categories
                FROM partner.branch aa
                inner join catalog.product_branch pb on pb.branch_id = aa.branch_id
                inner join catalog.product p on p.product_id = pb.product_id and p.active = true
                inner join catalog.base_product bp on bp.product_id = p.base_product_id
                inner join partner.partner pp on aa.partner_id  = pp.partner_id
                left join catalog.product_image pi on pi.product_image_id = p.image_default
                inner join authentication.profile ap on pp.user_id  = ap.user_id
                inner join partner.address a on a.branch_id = aa.branch_id
                 where aa.branch_id = '{branch_id}'
";

                    var response = connection.Query(sql).Select(x => new ListByProduct()
                    {

                        Description = x.description,
                        Identifier = x.identifier,
                        Name = x.name,
                        Type = x.type,
                        Admin_id = x.admin_id,
                        Image_default = x.image_default,
                        Url = x.url,
                        Branch = !string.IsNullOrEmpty(x.branch) ? JsonConvert.DeserializeObject<Branch>(x.branch) : new Branch(),
                        Product_id = x.product_id,
                        Price = x.sale_price,
                        Images = !string.IsNullOrEmpty(x.images) ? JsonConvert.DeserializeObject<List<Image>>(x.images) : new List<Image>(),
                        Categories = !string.IsNullOrEmpty(x.categories) ? JsonConvert.DeserializeObject<List<Category>>(x.categories) : new List<Category>()
                    }).ToList();

                    int totalRows = response.Count();
                    float totalPages = (float)totalRows / (float)filter.ItensPerPage;
                    totalPages = (float)Math.Ceiling(totalPages);
                    response = response.Skip((int)((filter.Page - 1) * filter.ItensPerPage)).Take((int)filter.ItensPerPage).ToList();


                    return new ProductsBranchOffer()
                    {
                        Pagination = new Pagination()
                        {
                            totalPages = (int)totalPages,
                            totalRows = totalRows
                        },
                        Products = response
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[OfferRepository - GetOffersByLocationPoint]: Error while retrieve offers by consumer and location point.");
                throw ex;
            }
        }
        static object GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property.GetValue(obj);
        }
        public Offer GetBranchOffersByLocationPoint(string latitude, string longitude, FilterOffer filter)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var sqlOffer = @$"select distinct aa.partner_id,
                               aa.branch_id,
                               b.branch_name,
                               ap.avatar,
                               (select coalesce((SUM(r.rating_value)/COUNT(DISTINCT(br.rating_id))),0) from reputation.branch_rating br 
                               join reputation.rating r on r.rating_id = br.rating_id where br.branch_id = aa.branch_id) ratings,
                               (SELECT round((logistics.distancia_km({latitude}, {longitude},a.latitude::decimal, a.longitude::decimal))::numeric, 1)) distance,
                               (select count(oi.product_id) from orders.orders o
                               join orders.orders_itens oi on oi.order_id = o.order_id
                               where o.branch_id = aa.branch_id) ordersNumbers
                                from logistics.actuation_area aa
                                join partner.branch b on b.branch_id = aa.branch_id 
                                join partner.address a on a.branch_id = aa.branch_id 
                                join partner.partner pp on aa.partner_id  = pp.partner_id
                                join authentication.profile ap on pp.user_id  = ap.user_id
                                inner join logistics.actuation_area_config c on c.actuation_area_id = aa.actuation_area_id  and c.start_hour::time <= '{DateTime.Now.AddHours(-3).TimeOfDay}'  AND '{DateTime.Now.AddHours(-3).TimeOfDay}' <= c.end_hour::time
                                join logistics.working_day wd on wd.actuation_area_config_id = c.actuation_area_config_id and wd.day_number in(date_part('dow', now()))
                                where logistics.ST_Intersects(logistics.ST_GeomFromText('POINT({longitude} {latitude})', 4326), aa.geometry) and aa.active = true and 
                        (unaccent(b.branch_name) ilike unaccent('%{filter.Filter}%'))
                                ";

                    var response = connection.Query<BranchOffer>(sqlOffer).ToList();

                    response = filter.SortOrder == SortOrder.asc ? response.OrderBy(x => GetPropertyValue(x, filter.OrderByBranch.ToString())).ToList() : response.OrderByDescending(x => GetPropertyValue(x, filter.OrderByBranch.ToString())).ToList();

                    int totalRows = response.Count();
                    float totalPages = (float)totalRows / (float)filter.ItensPerPage;
                    totalPages = (float)Math.Ceiling(totalPages);
                    response = response.Skip((int)((filter.Page - 1) * filter.ItensPerPage)).Take((int)filter.ItensPerPage).ToList();

                    connection.Close();
                    return new Offer()
                    {
                        Pagination = new Pagination()
                        {
                            totalPages = (int)totalPages,
                            totalRows = totalRows
                        },
                        Branches = response
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[OfferRepository - GetOffersByLocationPoint]: Error while retrieve offers by consumer and location point.");
                throw ex;
            }
        }

        public OfferProducts GetProductOffersByLocationPoint(string latitude, string longitude, FilterOffer filter)
        {
            try
            {

                var fieldInfo = filter.OrderByProduct.GetType().GetField(filter.OrderByProduct.ToString());
                DescriptionAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

                var orderby = attributes.First().Description;

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var sql = @$"with products as(

                                select distinct 
                                b.branch_id,
                                b.branch_name,
                                (SELECT round((logistics.distancia_km({latitude}, {longitude},a.latitude::decimal, a.longitude::decimal))::numeric, 1)) distance,
                                (select coalesce((SUM(r.rating_value)/COUNT(DISTINCT(br.rating_id))),0) from reputation.branch_rating br 
                                 join reputation.rating r on r.rating_id = br.rating_id where br.branch_id = aa.branch_id) ratings,
                                 p.base_product_id,
                                 p.product_id,
                                   p.name,
                                   p.sale_price,
                                   p.description,
                                   (select url from catalog.product_image where product_image_id = p.image_default ) url
                                    from logistics.actuation_area aa
                                    join partner.address a on a.branch_id = aa.branch_id 
                                    join logistics.actuation_area_config c on c.actuation_area_id = aa.actuation_area_id  and c.start_hour::time <= '{DateTime.Now.AddHours(-3).TimeOfDay}'  AND '{DateTime.Now.AddHours(-3).TimeOfDay}' <= c.end_hour::time
                                    join logistics.actuation_area_shipping s on s.actuation_area_config_id = c.actuation_area_config_id and (s.shipping_free = {filter.Shipping_free} or s.shipping_free = true)
                                join logistics.working_day wd on wd.actuation_area_config_id = c.actuation_area_config_id and wd.day_number in(date_part('dow', now()))
                                join catalog.product_branch pb on pb.branch_id = aa.branch_id
                                join catalog.product p on p.product_id = pb.product_id
                                join partner.branch b on b.branch_id = aa.branch_id ";

                    sql += string.IsNullOrEmpty(filter.Branch_ids) ? "" : @$"and aa.branch_id in ({filter.Branch_ids})";

                    sql += @$"join catalog.category_base_product cbp on cbp.product_id = p.base_product_id
                                            join catalog.category ca on ca.category_id = cbp.category_id ";

                    sql += string.IsNullOrEmpty(filter.Category_ids) ? "" : @$"and ca.category_id in ({filter.Category_ids})";

                    sql += @$"where logistics.ST_Intersects(logistics.ST_GeomFromText('POINT({longitude} {latitude})', 4326), aa.geometry) and aa.active = true
                                and p.active = true and p.price >= '{filter.Start_price}' and p.price <= '{filter.End_price}' and (unaccent(b.branch_name) ilike unaccent('%{filter.Filter}%') or 
                                unaccent(p.name) ilike unaccent('%{filter.Filter}%') or unaccent(p.description) ilike unaccent('%{filter.Filter}%') or unaccent(ca.description) ilike unaccent('%{filter.Filter}%'))
                                order by p.name asc
                                ) select count(oi.product_id) ordersNumbers, p.*,
                                (SELECT json_agg(category) from
                                    (select c.category_id, c.description, c.category_parent_id,(select cn.description from catalog.category cn 
                                    where cn.category_id = c.category_parent_id)category_parent_name
                                    FROM catalog.category_base_product cbp 
                                    join catalog.category c on c.category_id = cbp.category_id
                                    WHERE cbp.product_id = p.base_product_id
                                    ) category ) AS categories
                                from products p
                                left join orders.orders o on o.branch_id = p.branch_id
                                left join orders.orders_itens oi on oi.order_id = o.order_id and oi.product_id = p.product_id
                                where cast(p.ratings as text) ilike '{filter.Ratings}%' and p.distance <= '{filter.Distance}'
                                group by p.branch_id, p.branch_name, p.distance,p.ratings,p.base_product_id,p.product_id,p.name,
                                p.sale_price,p.description,p.url
                                order by {orderby} {filter.SortOrder}";

                    var response = connection.Query(sql).Select(x => new ProductOffer()
                    {
                        Branch_id = x.branch_id,
                        Branch_name = x.branch_name,
                        Product_id = x.product_id,
                        Name = x.name,
                        Description = x.description,
                        Distance = x.distance,
                        Ratings = x.ratings,
                        Ordersnumbers = x.ordersnumbers,
                        Price = x.sale_price,
                        Url = x.url,
                        Categories = !string.IsNullOrEmpty(x.categories) ? JsonConvert.DeserializeObject<List<Category>>(x.categories) : new List<Category>(),
                    }

                        ).ToList();

                    int totalRows = response.Count();
                    float totalPages = (float)totalRows / (float)filter.ItensPerPage;
                    totalPages = (float)Math.Ceiling(totalPages);
                    response = response.Skip((int)((filter.Page - 1) * filter.ItensPerPage)).Take((int)filter.ItensPerPage).ToList();

                    return new OfferProducts()
                    {
                        Products = response,
                        Pagination = new Pagination
                        {
                            totalPages = (int)totalPages,
                            totalRows = totalRows,
                        }

                    };
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool GetBranchByLocationPoint(string latitude, string longitude, Guid branch_id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var sql = $@"select aa.name
                        from logistics.actuation_area aa
                        inner join logistics.actuation_area_config c 
                        on c.actuation_area_id = aa.actuation_area_id  
                        and c.start_hour::time <= '{DateTime.Now.AddHours(-3).TimeOfDay}'  AND '{DateTime.Now.AddHours(-3).TimeOfDay}' <= c.end_hour::time
                        join logistics.working_day wd 
                        on wd.actuation_area_config_id = c.actuation_area_config_id 
                        and wd.day_number in(date_part('dow', now()))
                        where logistics.ST_Intersects(logistics.ST_GeomFromText('POINT({longitude} {latitude})', 4326), aa.geometry) 
                        and aa.branch_id = '{branch_id}' and aa.active = true";

                    var response = connection.Query<dynamic>(sql).ToList();

                    if (response.Count > 0) return true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[OfferRepository - GetBranchByLocationPoint]: Error when checking branch.");
                throw ex;
            }
        }

        public OfferFilters GetFiltersByLocationPoint(string latitude, string longitude, string filter)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var sql = @$"select 
                        (SELECT json_agg(branch) from
                    (select distinct b.branch_id, b.branch_name
                    FROM partner.branch b 
                    join logistics.actuation_area aa on aa.branch_id = b.branch_id
                    join catalog.product_branch pb on pb.branch_id = b.branch_id
                    join catalog.product p on p.product_id = pb.product_id
                    join catalog.category_base_product cbp on cbp.product_id = p.base_product_id
                    join catalog.category c on c.category_id = cbp.category_id
                    where logistics.ST_Intersects(logistics.ST_GeomFromText('POINT({longitude} {latitude})', 4326), aa.geometry)and
                    (unaccent(b.branch_name) ilike unaccent('%{filter}%') or 
                    unaccent(p.name) ilike unaccent('%{filter}%') or unaccent(p.description) ilike unaccent('%{filter}%') or unaccent(c.description) ilike unaccent('%{filter}%'))
                    ) branch ) AS branchs,

                        (SELECT json_agg(category) from
                    (select distinct c.category_id, c.description
                    FROM catalog.category_base_product cbp 
                    join catalog.category c on c.category_id = cbp.category_id
                    join catalog.product p on p.base_product_id = cbp.product_id
                    join catalog.product_branch pb on pb.product_id = p.product_id
                    join logistics.actuation_area aa on aa.branch_id = pb.branch_id
                    where logistics.ST_Intersects(logistics.ST_GeomFromText('POINT({longitude} {latitude})', 4326), aa.geometry)
                    and
                    (unaccent(b.branch_name) ilike unaccent('%{filter}%') or 
                    unaccent(p.name) ilike unaccent('%{filter}%') or unaccent(p.description) ilike unaccent('%{filter}%') or unaccent(c.description) ilike unaccent('%{filter}%'))
                    ) category ) AS categories,

                    (select MAX(p.sale_price) price_maximum from catalog.product p
                    join catalog.product_branch pb on pb.product_id = p.product_id
                    join logistics.actuation_area aa on aa.branch_id = pb.branch_id
                    join partner.branch b on b.branch_id = aa.branch_id
                    join catalog.category_base_product cbp on cbp.product_id = p.base_product_id
                    join catalog.category c on c.category_id = cbp.category_id
                    where logistics.ST_Intersects(logistics.ST_GeomFromText('POINT({longitude} {latitude})', 4326), aa.geometry) and
                    (unaccent(b.branch_name) ilike unaccent('%{filter}%') or 
                    unaccent(p.name) ilike unaccent('%{filter}%') or unaccent(p.description) ilike unaccent('%{filter}%') or unaccent(c.description) ilike unaccent('%{filter}%'))
                    ),

                    (SELECT max(round((logistics.distancia_km({latitude}, {longitude},a.latitude::decimal, a.longitude::decimal))::numeric, 1))
                    from logistics.actuation_area aa
                    join partner.address a on a.branch_id = aa.branch_id
                    join catalog.product_branch pb on pb.branch_id = aa.branch_id
                    join catalog.product p on pb.product_id = p.product_id
                    join catalog.category_base_product cbp on cbp.product_id = p.base_product_id
                    join catalog.category c on c.category_id = cbp.category_id 
                    where logistics.ST_Intersects(logistics.ST_GeomFromText('POINT({longitude} {latitude})', 4326), aa.geometry) and
                    (unaccent(b.branch_name) ilike unaccent('%{filter}%') or 
                    unaccent(p.name) ilike unaccent('%{filter}%') or unaccent(p.description) ilike unaccent('%{filter}%') or unaccent(c.description) ilike unaccent('%{filter}%'))
                    ) distance_maximum

                        from logistics.actuation_area aa
                        join logistics.actuation_area_config aac on aa.actuation_area_id = aac.actuation_area_id
                        join catalog.product_branch pb on pb.branch_id = aa.branch_id
                        join catalog.product p on p.product_id = pb.product_id
                        join partner.branch b on b.branch_id = aa.branch_id
                        join catalog.category_base_product cbp on cbp.product_id = p.base_product_id
                        join catalog.category c on c.category_id = cbp.category_id 
                    where logistics.ST_Intersects(logistics.ST_GeomFromText('POINT({longitude} {latitude})', 4326), aa.geometry) and
                    (unaccent(b.branch_name) ilike unaccent('%{filter}%') or 
                    unaccent(p.name) ilike unaccent('%{filter}%') or unaccent(p.description) ilike unaccent('%{filter}%') or unaccent(c.description) ilike unaccent('%{filter}%'))
                        limit 1";

                    var response = connection.Query(sql).Select(x => new OfferFilters()
                    {
                        Branchs = !string.IsNullOrEmpty(x.branchs) ? JsonConvert.DeserializeObject<List<BranchFilters>>(x.branchs) : new List<BranchFilters>(),
                        Categories = !string.IsNullOrEmpty(x.categories) ? JsonConvert.DeserializeObject<List<CategoryFilters>>(x.categories) : new List<CategoryFilters>(),
                        Price_maximum = x.price_maximum,
                        Distance_maximum = x.distance_maximum

                    }).FirstOrDefault();

                    return response;
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
