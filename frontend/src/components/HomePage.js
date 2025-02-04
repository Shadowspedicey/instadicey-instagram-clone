import { useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { startLoading, stopLoading } from "../state/actions/isLoading";
import PostWindow from "./Posts/PostWindow";

const HomePage = () =>
{
	const postsRef = useRef();
	const dispatch = useDispatch();
	const currentUser = useSelector(state => state.currentUser.info);
	const [maxDate, setMaxDate] = useState(null);
	const [scroll, setScroll] = useState(0);
	const [olderPosts, setOlderPosts] = useState(null);
	const [postsToDisplay, setPostsToDisplay] = useState(null);

	const handleScroll = () =>
	{
		const winScroll = document.body.scrollTop || document.documentElement.scrollTop;
		const height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
  	setScroll(winScroll / height * 100);
	};
	useEffect(handleScroll, []);

	useEffect(() => document.title = "Instadicey", []);
	useEffect(() =>
	{
		const getPosts = async () =>
		{
			if (!currentUser) return;
			dispatch(startLoading());
			// TODO: Make sure user following count > 0 and if so, get user feed (not older than 3 days) and set it (setPostsToDisplay)
			dispatch(stopLoading());
		};
		getPosts();
	}, [currentUser, dispatch]);

	useEffect(() =>
	{
		if (scroll < 75 || !maxDate || olderPosts) return;

		const getOlderPosts = async () =>
		{
			// TODO: Make sure user following count > 0 and if so, get posts older than 3 days and set them (setOlderPosts)
		};
		getOlderPosts();
	// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [scroll, currentUser, maxDate]);

	useEffect(() =>
	{
		window.addEventListener("scroll", handleScroll);
		return () =>
		{
			window.removeEventListener("scroll", handleScroll);
		};
	}, [postsRef]);

	if (!postsToDisplay) return null;
	return(
		<div className="home-page">
			{
				postsToDisplay &&
				<div className="posts" ref={postsRef}>
					{
						postsToDisplay.map(post =>
							<PostWindow postID={post.id} isVertical key={post.id}/>)
					}
				</div>
			}
			{ olderPosts &&
					<div className="older-posts posts" ref={postsRef}>
						<h2>Showing posts older than 3 days</h2>
						{
							olderPosts.map(post =>
								<PostWindow postID={post.id} isVertical key={post.id}/>)
						}
					</div>
			}
		</div>
	);
};

export default HomePage;
