Name:           rhino-compute
Version:        9.0.0
Release:        1%{?dist}
Summary:        REST geometry server based on RhinoCommon and headless Rhino

License:        MIT
URL:            https://github.com/mcneel/compute.rhino3d/blob/9.x/LICENSE.md
Source0:        %{name}_%{version}-wip.tar.gz
Source1:        rhino-compute.service
Source2:        environment.example
Source3:        rhino-compute-start.sh

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

# Install startup script for non-systemd environments
install -m 0755 %{SOURCE3} %{buildroot}/usr/bin/rhino-compute-start

# Install systemd service file (if systemd is available)
if [ -d /usr/lib/systemd/system ] || [ -d /lib/systemd/system ]; then
    mkdir -p %{buildroot}%{_unitdir}
    install -m 0644 %{SOURCE1} %{buildroot}%{_unitdir}/rhino-compute.service
fi

# Install environment example file
mkdir -p %{buildroot}%{_sysconfdir}/rhino-compute
install -m 0644 %{SOURCE2} %{buildroot}%{_sysconfdir}/rhino-compute/environment.example

# Create log directory
mkdir -p %{buildroot}/var/log/rhino-compute

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
/usr/bin/rhino-compute-start
%dir %{_sysconfdir}/rhino-compute
%config(noreplace) %{_sysconfdir}/rhino-compute/environment.example
%dir %attr(0755, rhino-compute, rhino-compute) /var/log/rhino-compute
# Service file only if systemd directory exists
%if 0%{?_unitdir:1}
%{_unitdir}/rhino-compute.service
%endif

# ----------------------------------------------------------------
# Pre-install: create service user if it doesn't exist
%pre
getent group rhino-compute >/dev/null || groupadd -r rhino-compute
if ! getent passwd rhino-compute >/dev/null; then
    useradd -r -g rhino-compute -d /var/lib/rhino-compute -s /sbin/nologin \
    -c "Rhino Compute Service" rhino-compute
else
    # Update existing user's home directory if incorrect
    CURRENT_HOME=$(getent passwd rhino-compute | cut -d: -f6)
    if [ "$CURRENT_HOME" != "/var/lib/rhino-compute" ]; then
        usermod -d /var/lib/rhino-compute rhino-compute
    fi
fi
exit 0

# ----------------------------------------------------------------
# Post-install: reload systemd and inform user
%post
# Only use systemd if it's available
if [ -d /run/systemd/system ]; then
    %systemd_post rhino-compute.service
fi

# Create and set ownership of state directory
mkdir -p /var/lib/rhino-compute
chown rhino-compute:rhino-compute /var/lib/rhino-compute
chmod 755 /var/lib/rhino-compute

# Create and set ownership of log directory
mkdir -p /var/log/rhino-compute
chown rhino-compute:rhino-compute /var/log/rhino-compute
chmod 755 /var/log/rhino-compute

cat <<EOF

  # # # # # # # # # # # # # # # # # # # # #
  #                                       #
  #       R H I N O   C O M P U T E       #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #

Rhino Compute has been installed successfully!

Next steps to get started:

1. Configure your API key:
   sudo cp /etc/rhino-compute/environment.example /etc/rhino-compute/environment
   sudo nano /etc/rhino-compute/environment
   (Set RHINO_TOKEN to your API key)

2. Start the service:
   sudo systemctl start rhino-compute

Other useful commands:

- Stop the service:
   sudo systemctl stop rhino-compute

- Enable automatic startup on boot:
   sudo systemctl enable rhino-compute

- Check the status:
   sudo systemctl status rhino-compute

- Follow logs in real-time:
   sudo journalctl -u rhino-compute -f

---

For non-systemd environments (i.e. docker containers):

1. Configure your API key:
   cp /etc/rhino-compute/environment.example /etc/rhino-compute/environment
   nano /etc/rhino-compute/environment
   (Set RHINO_TOKEN to your API key)
   save and close the file

2. Start rhino-compute: 
   rhino-compute-start

---

Logs are written to: /var/log/rhino-compute

EOF

# ----------------------------------------------------------------
# Pre-uninstall: stop and disable the service
%preun
if [ -d /run/systemd/system ] && [ -f %{_unitdir}/rhino-compute.service ]; then
    %systemd_preun rhino-compute.service
fi

# ----------------------------------------------------------------
# Post-uninstall: reload systemd
%postun
if [ -d /run/systemd/system ]; then
    %systemd_postun_with_restart rhino-compute.service
fi

# ----------------------------------------------------------------
%changelog
* Tue Feb 10 2026 Luis Fraguada <luis@mcneel.com> - 9.0.0-1
- Initial rhino-compute RPM