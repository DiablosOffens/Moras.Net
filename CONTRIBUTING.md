Guidelines
==========

When contributing to the Moras.Net project, please follow the [Moras.Net Coding Guidelines](CODESTYLE.md).  We are working to introduce this coding style to the project.  Please make your pull requests conform to these guidelines.

Etiquette
=========

In general, we do not accept pull requests that merely shuffle code around, split classes in multiple files, reindent the code or are the result of running a refactoring tool on the source code.  This is done for three reasons:

* We have our own coding guidelines
* Some modules are imported from upstream sources and we want to respect their coding guidelines
* It destroys valuable history that is often used to investigate bugs, regressions and problems

License
=======

The Moras.Net project uses the GNU General Public License v2.0.  See `LICENSE` for more details.  Some third-party libraries used by Moras.Net may be under a different license.  Please refer to those libraries for details on the license they use.

Submitting Patches
==================

Moras.Net consists of only one branch:

* `master` is the stable branch from which releases are made.  Pull requests must be made against the master branch.  Direct commits to the master branch will not be accepted.

The process for making a pull request is generally as follows:

1. Make a feature branch from `master` for the change.
2. Edit, build and test the feature.
3. Commit to your local repository.
4. Push the feature branch to your GitHub fork.
5. Create the pull request.

If you need to make changes to the pull request, simply repeat steps 2-4.  Adding commits to that feature branch in your fork will automatically add the change to the pull request.

The majority of code in Moras.Net is cross-platform and must build and behave correctly on all supported platforms.  As long as there is no automated build system, all pull requests to Moras.Net will be manually checked and built on a single platform by pull request reviewer and then reported back with any build errors.

Once a pull request has been accepted, your feature branch can be deleted if desired.
