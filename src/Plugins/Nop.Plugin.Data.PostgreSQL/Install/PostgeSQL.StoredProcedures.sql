CREATE OR REPLACE FUNCTION public.nop_getnotnullnotempty(
    p1 text DEFAULT NULL::text,
    p2 text DEFAULT NULL::text)
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
AS $BODY$

BEGIN
    IF p1 IS NULL THEN
        return p2;
    END IF;

    IF p1 ='' THEN
        return p2;
    END IF;

    return p1;
END
$BODY$;
----NEXT----
CREATE OR REPLACE FUNCTION public.nop_padright(
    source integer,
    symbol text,
    length integer)
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE
AS $BODY$

 DECLARE index_name text;
BEGIN
    RETURN right(repeat("symbol", "length") || rtrim("source"::text), "length");
END

$BODY$;
----NEXT----
CREATE OR REPLACE FUNCTION public.nop_splitstring_to_table(
    string text,
    delimiter character)
    RETURNS TABLE(data text) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$

DECLARE startindex INT;
DECLARE endtindex INT;
DECLARE tempstring TEXT;
BEGIN
    tempstring := string;
    startindex := 1;
    endtindex := position(delimiter in tempstring);
    WHILE startindex < length(tempstring) + 1 LOOP
        IF endtindex = 0 THEN
            endtindex := length(tempstring) + 1;
        END IF;
        data := SUBSTRING(tempstring, startindex, endtindex - startindex);

        tempstring := SUBSTRING(tempstring, endtindex + 1, length(tempstring)+1);
        endtindex := position(delimiter in tempstring);
        RETURN NEXT;
    END LOOP;
END
$BODY$
----NEXT----
CREATE OR REPLACE FUNCTION public.categoryloadall(
    showhidden boolean,
    name text,
    storeid integer,
    customerroleids text)
    RETURNS SETOF "Category" 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$

DECLARE FilteredCustomerRoleIdsCount INT;
DECLARE lengthId INT;
DECLARE lengthOrder INT;

BEGIN
DROP TABLE IF EXISTS "FilteredCustomerRoleIds";
    DROP TABLE IF EXISTS "OrderedCategoryIds";
    CustomerRoleIds := '';
    CREATE TEMP TABLE  "FilteredCustomerRoleIds"
    (
        CustomerRoleId INT NOT NULL
    );
    INSERT INTO "FilteredCustomerRoleIds" (CustomerRoleId)
    SELECT CAST(data AS INT) FROM nop_splitstring_to_table (CustomerRoleIds, ',');
    SELECT COUNT(1) into FilteredCustomerRoleIdsCount FROM "FilteredCustomerRoleIds";

    --ordered categories
    CREATE TEMP TABLE "OrderedCategoryIds"
    (
        Id int GENERATED ALWAYS AS IDENTITY  NOT NULL,
        CategoryId int NOT NULL
    );

    lengthId := (SELECT length(MAX("Id")::text) FROM "Category");
    lengthOrder := (SELECT length(MAX("DisplayOrder")::text) FROM "Category");

    WITH RECURSIVE  "CategoryTree"
    AS (SELECT "Category"."Id" AS "Id", nop_padright ("Category"."DisplayOrder", '0', lengthOrder) || '-' || nop_padright ("Category"."Id", '0', lengthId) AS "Order"
        FROM "Category" WHERE "Category"."ParentCategoryId" = 0
        UNION ALL
        SELECT "Category"."Id" AS "Id", "CategoryTree"."Order" || '|' || nop_padright ("Category"."DisplayOrder", '0', lengthOrder) || '-' || nop_padright ("Category"."Id", '0', lengthId) AS "Order"
        FROM "Category"
        INNER JOIN "CategoryTree" ON "CategoryTree"."Id" = "Category"."ParentCategoryId")
    INSERT INTO "OrderedCategoryIds" (CategoryId)
    SELECT "Category"."Id"
    FROM "CategoryTree"
    RIGHT JOIN "Category" ON "CategoryTree"."Id" = "Category"."Id"

        --filter results
    WHERE "Category"."Deleted" = FALSE
    AND (ShowHidden = '1' OR "Category"."Published" = TRUE)
    AND (Name IS NULL OR Name = '' OR "Category"."Name" LIKE ('%' || Name || '%'))
    AND (ShowHidden = '1' OR FilteredCustomerRoleIdsCount  = 0 OR "Category"."SubjectToAcl" = FALSE
        OR EXISTS (SELECT 1 FROM "FilteredCustomerRoleIds" roles WHERE roles.CustomerRoleId IN
            (SELECT acl."CustomerRoleId" FROM "AclRecord" acl  WHERE acl."EntityId" = "Category"."Id" AND acl."EntityName" = 'Category')
        )
    )
    AND (StoreId = 0 OR "Category"."LimitedToStores" = FALSE
        OR EXISTS (SELECT 1 FROM "StoreMapping" sm
            WHERE sm."EntityId" = "Category"."Id" AND sm."EntityName" = 'Category' AND sm."StoreId" = StoreId
        )
    )
    ORDER BY COALESCE("CategoryTree"."Order", '1');

    --paging
    RETURN QUERY SELECT "Category".* FROM "OrderedCategoryIds" AS "Result" INNER JOIN "Category" ON "Result".CategoryId = "Category"."Id"
    ORDER BY "Result".Id;

    DROP TABLE "FilteredCustomerRoleIds";
    DROP TABLE "OrderedCategoryIds";
END

$BODY$;
----NEXT----
CREATE OR REPLACE FUNCTION public.categoryloadallcount(
    showhidden boolean DEFAULT false,
    name text DEFAULT NULL::text,
    storeid integer DEFAULT 0,
    customerroleids text DEFAULT NULL::text)
    RETURNS TABLE("Value" integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$

DECLARE rowcount INT;
BEGIN
    rowcount := (SELECT COUNT(*) FROM public.categoryloadall(showhidden, name, storeid, customerroleids));
    RETURN QUERY SELECT rowcount ;
END

$BODY$;
----NEXT----
CREATE OR REPLACE FUNCTION public.categoryloadallpaged(
    showhidden boolean DEFAULT false,
    name text DEFAULT NULL::text,
    storeid integer DEFAULT 0,
    customerroleids text DEFAULT NULL::text,
    pageindex integer DEFAULT 0,
    pagesize integer DEFAULT 2147483644)
    RETURNS SETOF "Category" 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$

DECLARE rowcount INT;
BEGIN
    RETURN QUERY SELECT * FROM public.categoryloadall(showhidden, name, storeid, customerroleids)
    WHERE ("Id" > PageSize * PageIndex AND "Id" <= PageSize * (PageIndex + 1));
END

$BODY$;
----NEXT----
CREATE OR REPLACE FUNCTION public.producttagcountloadall(
    storeid integer)
    RETURNS TABLE(producttagid integer, productcount bigint) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$

 DECLARE index_name text;
BEGIN
    RETURN QUERY SELECT "pt"."Id" as "ProductTagId", COUNT("p"."Id") as "ProductCount"
    FROM "ProductTag" "pt"
    LEFT JOIN "Product_ProductTag_Mapping" "pptm" ON "pt"."Id" = "pptm"."ProductTag_Id"
    LEFT JOIN "Product" p ON "pptm"."Product_Id" = "p"."Id"
    WHERE
        ("p"."Deleted" = FALSE)
        AND ("p"."Published" = TRUE)
        AND (StoreId = 0 or ("p"."LimitedToStores" = FALSE OR EXISTS (
            SELECT 1 FROM "StoreMapping" "sm"
            WHERE "sm"."EntityId" = "p"."Id" AND "sm"."EntityName" = 'Product' and "sm"."StoreId"=0
            )))
    GROUP BY "pt"."Id"
    ORDER BY "pt"."Id";

END

$BODY$;
----NEXT----
CREATE OR REPLACE FUNCTION public.deleteguests(
    createdfromutc timestamp without time zone,
    createdtoutc timestamp without time zone,
    onlywithoutshoppingcart boolean DEFAULT true)
    RETURNS TABLE("Value" integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$

DECLARE rowcount INT;
BEGIN
    DROP TABLE IF EXISTS tmp_guests;
    CREATE TEMP TABLE  tmp_guests
    (
        CustomerId INT NOT NULL
    );
    
    INSERT INTO tmp_guests (CustomerId)
    SELECT "Id" FROM "Customer" c
    WHERE
    --created from
    ((createdfromutc is null) OR (c."CreatedOnUtc" > createdfromutc))
    AND
    --created to
    ((createdtoutc is null) OR (c."CreatedOnUtc" < createdtoutc))
    AND
    --shopping cart items
    ((onlywithoutshoppingcart = FALSE) OR (NOT EXISTS(SELECT 1 FROM "ShoppingCartItem" sci  inner join "Customer"  on sci."CustomerId"=c."Id")))
    AND
    --guests only
    (EXISTS(SELECT 1 FROM "Customer_CustomerRole_Mapping" ccrm inner join "Customer" on ccrm."Customer_Id"=c."Id" inner join "CustomerRole" cr on cr."Id"=ccrm."CustomerRole_Id" WHERE cr."SystemName" = N'Guests'))
    AND
    --no orders
    (NOT EXISTS(SELECT 1 FROM "Order" o inner join "Customer" on o."CustomerId"=c."Id"))
    AND
    --no blog comments
    (NOT EXISTS(SELECT 1 FROM "BlogComment" bc inner join "Customer" on bc."CustomerId"=c."Id"))
    AND
    --no news comments
    (NOT EXISTS(SELECT 1 FROM "NewsComment" nc inner join "Customer" on nc."CustomerId"=c."Id"))
    AND
    --no product reviews
    (NOT EXISTS(SELECT 1 FROM "ProductReview" pr inner join "Customer" on pr."CustomerId"=c."Id"))
    AND
    --no product reviews helpfulness
    (NOT EXISTS(SELECT 1 FROM "ProductReviewHelpfulness" prh inner join "Customer" on prh."CustomerId"=c."Id"))
    AND
    --no poll voting
    (NOT EXISTS(SELECT 1 FROM "PollVotingRecord" pvr inner join "Customer" on pvr."CustomerId"=c."Id"))
    AND
    --no forum topics 
    (NOT EXISTS(SELECT 1 FROM "Forums_Topic" ft inner join "Customer" on ft."CustomerId"=c."Id"))
    AND
    --no forum posts 
    (NOT EXISTS(SELECT 1 FROM "Forums_Post" fp inner join "Customer" on fp."CustomerId"=c."Id"))
    AND
    --no system accounts
    (c."IsSystemAccount" = FALSE);

    DELETE FROM "Customer"
    WHERE "Id" IN (SELECT CustomerId FROM tmp_guests);

    --delete attributes
    DELETE FROM "GenericAttribute"
    WHERE ("EntityId" IN (SELECT CustomerId FROM tmp_guests))
    AND
    ("KeyGroup" = N'Customer');

        rowcount := (SELECT COUNT(*) FROM tmp_guests);
    RETURN QUERY SELECT rowcount ;
END

$BODY$;