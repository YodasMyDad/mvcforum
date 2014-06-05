![MVCForum](http://chat.mvcforum.com/installer/content/images/logo.png)

MVCForum - Fully Featured ASP.NET MVC 5 Forum
========

MVCForum is a fully featured enterprise level ASP.NET MVC 5 discussion board/forum with multiple responsive themes. The software has features similar to that seen on StackOverFlow plus LOTS more (See list below). Along with MVC it’s built using Unity & Entity Framework 6.1 and is super easy to extend and add your own features.

Current Features Include

- Multi-Lingual / Localisation
- Points System
- Moderate Topics & Posts
- Badge System (Like StackOverflow)
- Permission System
- Roles
- Mark Posts As Solution
- Vote Up / Down Posts (View who voted up your posts)
- Easy Logging
- Global and Weekly points Leader board
- Responsive Bootstrap Themes
- Latest Activity
- Simple API
- Custom Events (Hook into them easily)
- Lucene.Net Search
- Polls
- Mobile Version (Theme able)
- Spam Prevention
- Facebook Login
- Private Messages
- Member  & Post Reporting 
- Lucene Indexing
- Batch tools and reporting
- Plus loads more!!

We are looking for feedback on improvements, bugs and new features so please give it a spin and let us know what you think. MVCForum is designed and developed by the [team at Aptitude](http://www.aptitude.co.uk)

## Documentation ##

Most documentation can be found on our website, including videos and in our blog

[http://www.mvcforum.com](http://www.mvcforum.com)

Our current support forum for bugs and questions (Which is also running on the latest MVCForum)

[http://chat.mvcforum.com](http://chat.mvcforum.com)

## Installing ##

To get the forum up and running from the compiled release. Just edit the web.config and point it to a pre-existing blank database. The installer should then just kick in.

You can also install via the Web Platform installer

[<img src="http://www.mvcforum.com/media/9168/button_install_green.png">](http://www.microsoft.com/web/gallery/install.aspx?appid=MVCForum)

and also nuget

[https://www.nuget.org/packages/TheMVCForum/](https://www.nuget.org/packages/TheMVCForum/)

    Install-Package TheMVCForum

**Upgrading**

Our installer 'should' upgrade the database between versions (It can't jump multiple versions at present). But upgrading the database manually between versions should be pretty easy. We keep all changes and notes between versions in the following file which is in the source

Notes > VersionDbChanges

And the upgrade SQL scripts for each version can be found in

Installer > Db > *Version Number* > Upgrade

## Screenshots ##

![Home Page](http://www.mvcforum.com/img/screens/homepage.png)

----------

![Badges](http://www.mvcforum.com/img/screens/badges.png)

----------

![Markdown](http://www.mvcforum.com/img/screens/markdown.png)

----------

![Activity](http://www.mvcforum.com/img/screens/activity.png)
