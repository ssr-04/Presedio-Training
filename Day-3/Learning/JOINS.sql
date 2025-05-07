use pubs;

-- Publisher details who havent published any books.
SELECT
    p.pub_id,
    p.pub_name,
    p.city,
    p.state,
    p.country
FROM
    publishers AS p
LEFT JOIN
    titles AS t ON p.pub_id = t.pub_id 
WHERE
    t.pub_id IS NULL;

-- Select the author_id for all the books and print author_id and book name
SELECT 
	au_id AS Author_ID, T.title AS Book_Name
FROM 
	titleauthor TA
JOIN 
	titles T ON TA.title_id = T.title_id;

-- Let's get with the name
SELECT 
	CONCAT(A.au_fname, ' ', A.au_lname) Author_Name, TI.title Book_Name
FROM 
	authors A
JOIN
	titleauthor TA ON A.au_id = TA.au_id
JOIN
	titles TI on TI.title_id = TA.title_id
ORDER BY
	1; --Refers to the Author_name as it's first in the selection (Ordinal number)