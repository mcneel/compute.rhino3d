Name:           rhino-compute
Version:        9.0.0
Release:        1%{?dist}
Summary:        REST geometry server based on RhinoCommon and headless Rhino

License:        MIT
URL:            https://github.com/mcneel/compute.rhino3d/blob/9.x/LICENSE.md
Source0:        %{name}_%{version}-wip.tar.gz


%global __requires_exclude ^liblttng-ust\\.so\\.0.*$

Requires:       rhino3d

%description
Rhino Compute for Linux.

# ----------------------------------------------------------------
# Prep: unpack the tarball
%prep
%setup -q -n rhino-compute_9.0.0-wip

# ----------------------------------------------------------------
# Install: copy files to buildroot
%install
rm -rf %{buildroot}

# Create install prefix
mkdir -p %{buildroot}/usr/lib/rhino-compute

# Copy all files from tarball into /usr/lib/rhino-compute
cp -a LICENSE.md compute.geometry rhino.compute %{buildroot}/usr/lib/rhino-compute/

# Ensure executable is actually executable
chmod +x %{buildroot}/usr/lib/rhino-compute/rhino.compute/rhino.compute

# Optional: create symlink for launcher
mkdir -p %{buildroot}/usr/bin
ln -s /usr/lib/rhino-compute/rhino.compute/rhino.compute %{buildroot}/usr/bin/rhino-compute

# ----------------------------------------------------------------
%clean
rm -rf %{buildroot}

# ----------------------------------------------------------------
# Files included in the RPM
%files
%defattr(-,root,root,-)
%license LICENSE.md
/usr/lib/rhino-compute
/usr/bin/rhino-compute

# ----------------------------------------------------------------
%changelog
* Tue Feb 10 2026 Luis Fraguada <luis@mcneel.com> - 9.0.0-1
- Initial rhino-compute RPM