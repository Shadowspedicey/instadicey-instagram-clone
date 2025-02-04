import { useEffect, useState } from "react";
import { useParams } from "react-router";
import { useDispatch } from "react-redux";
import { startLoading, stopLoading } from "../../state/actions/isLoading";
import PostWindow from "./PostWindow";
import PostCard from "./PostCard";
import BrokenPage from "../BrokenPage";
import "./post-page.css";

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

	const getPostData = async () =>
	{
		try
		{
			dispatch(startLoading());
			// TODO: Get post data by ID and set it
			// getMorePosts(post);
		} catch (err)
		{
			console.error(err);
		}
		dispatch(stopLoading());
	};

	const getMorePosts = async postData =>
	{
		// TODO: Gets other posts (max 6) from the same user
	};

	useEffect(() =>
	{
		getPostData();
	// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [postID]);

	if (!postData) return <BrokenPage/>;
	else return(
		<div className="post-page">
			{
				isSmallScreen
					? <PostWindow postID={postID} isVertical/>
					: <PostWindow postID={postID}/>
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
