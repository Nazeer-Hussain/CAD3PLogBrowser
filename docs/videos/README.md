This folder holds short (30-90 second) feature demo video files referenced by docs/videos.json.

How to add a new video:
1. Drop an .mp4 (or .webm) file in this folder, e.g. call-tree-demo.mp4.
   Keep clips short (30-90s) and file sizes reasonable for GitHub Pages hosting.
2. Add an entry to docs/videos.json:
   {
       "title": "Call Tree walkthrough",
       "description": "Shows how the call tree color-codes slow calls and expands to matching ENTER/EXIT.",
       "src": "videos/call-tree-demo.mp4",
       "embedUrl": "",
       "durationSeconds": 42
   }
3. Alternatively, if the video is hosted elsewhere (YouTube, etc.), leave "src" empty and set
   "embedUrl" to an embeddable player URL (e.g. https://www.youtube.com/embed/VIDEO_ID) instead.

docs/videos.html reads this JSON at runtime and renders a card per entry, so no HTML edits
are needed to add, remove, or reorder videos.
