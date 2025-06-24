import { useEffect, useState } from "react";
import { useParams } from "react-router";
import { useDispatch } from "react-redux";
import { startLoading, stopLoading } from "../../state/actions/isLoading";
import PostWindow from "./PostWindow";
import PostCard from "./PostCard";
import BrokenPage from "../BrokenPage";
import "./post-page.css";
import { backend } from "../../config";

const PostPage = () =>
{
	const { postID } = useParams();
	const dispatch = useDispatch();
	const [postData, setPostData] = useState(null);
	const [morePosts, setMorePosts] = useState(null);
	const [isSmallScreen, setIsSmallScreen] = useState(false);
	const handleResize = () => window.innerWidth < 1024 ? setIsSmallScreen(true) : setIsSmallScreen(false);
	useEffect(() =>
	{
		handleResize();
		window.addEventListener("resize", handleResize);
		return () => window.removeEventListener("resize", handleResize);
	}, []);

	useEffect(() => document.title = "Instadicey", []);

	const getPostData = async () =>
	{
		try
		{
			const result = await fetch(`${backend}/post/${postID}`);
			const resultJSON = await result.json();

			if (!result.ok)
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });

			// Required because JS UTC dates have to end with Z
			// resultJSON.comments =resultJSON.comments.map(c => {
			// 	const newComment = {...c, createdAt: c.createdAt + "Z"};
			// 	return newComment;
			// });
			setPostData(resultJSON);
			await getMorePosts();
		} catch (err)
		{
			console.error(err);
		}
	};

	const getMorePosts = async () =>
	{
		try {
			const result = await fetch(`${backend}/post/${postID}/more`);
			const resultJSON = await result.json();
			if (!result.ok)
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });

			setMorePosts(resultJSON);
		} catch {

		}
	};

	useEffect(() =>
	{
		const getData = async () => {
			dispatch(startLoading());
			await getPostData();
			dispatch(stopLoading());
		};
		getData();
	// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [postID]);

	if (!postData) return <BrokenPage/>;
	else return(
		<div className="post-page">
			{
				isSmallScreen
					? <PostWindow post={postData} refreshPost={getPostData} isVertical/>
					: <PostWindow post={postData} refreshPost={getPostData} />
			}
			<div className="more-posts">
				<header>More posts from this user</header>
				{ morePosts &&
					<div className="post-cards-container">
						{ morePosts.map(post => <PostCard post={post} key={post.id}/>) }
					</div>
				}
			</div>
		</div>
	);
};

export default PostPage;
